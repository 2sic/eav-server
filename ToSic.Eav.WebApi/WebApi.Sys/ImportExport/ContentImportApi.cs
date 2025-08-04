using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Apps.Sys.LogSettings;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.ImportExport.Sys.XmlList;
using ToSic.Eav.Serialization.Sys;
using ToSic.Eav.WebApi.Sys.Dto;
#if NETFRAMEWORK
using System.Web.Http;
#else
using Microsoft.AspNetCore.Mvc;
#endif

namespace ToSic.Eav.WebApi.Sys.ImportExport;

/// <inheritdoc />
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ContentImportApi(
    LazySvc<ImportListXml> importListXml,
    LazySvc<JsonSerializer> jsonSerializerLazy,
    AppCachePurger appCachePurger,
    GenWorkDb<WorkEntitySave> workEntSave,
    IAppsCatalog appsCatalog,
    IAppReaderFactory appReaders,
    DataImportLogSettings importLogSettings
) : ServiceBase("Api.EaCtIm", connect: [workEntSave, importListXml, jsonSerializerLazy, appCachePurger, appsCatalog, appReaders, importLogSettings])
{
    private IAppReader _appReader = null!;

    public ContentImportApi Init(int appId)
    {
        var l = Log.Fn<ContentImportApi>($"app: {appId}");
        _appReader = appReaders.Get(appId);
        return l.Return(this);
    }

    #region Detailed Logging

    [field: AllowNull, MaybeNull]
    private LogSettings LogSettings => field ??= importLogSettings.GetLogSettings();

    #endregion


    [HttpPost]
    public ContentImportResultDto XmlPreview(ContentImportArgsDto args)
    {
        var l = Log.Fn<ContentImportResultDto>("eval content - start" + args.DebugInfo);

        var import = GetXmlImport(args);
        if (import.ErrorLog.HasErrors)
            l.ReturnAsError(new(!import.ErrorLog.HasErrors, import.ErrorLog.Errors));

        var stats = import.Preparations;
        var result = new ContentImportResultDto(!import.ErrorLog.HasErrors, new ImportStatisticsDto
            {
                AmountOfEntitiesCreated = stats.EntitiesToCreate,
                AmountOfEntitiesDeleted = stats.DeleteCount,
                AmountOfEntitiesUpdated = stats.EntitiesToUpdate,
                AttributeNamesInDocument = stats.AttributeNamesInDocument,
                AttributeNamesInContentType = stats.AttributeNamesInContentType,
                AttributeNamesNotImported = stats.AttributeNamesNotImported,
                DocumentElementsCount = stats.Count,
                LanguagesInDocumentCount = stats.LanguagesInDocument.Count()
            });
        return l.Return(result);
    }

    [HttpPost]
    public ContentImportResultDto XmlImport(ContentImportArgsDto args)
    {
        var l = Log.Fn<ContentImportResultDto>(args.DebugInfo);
        
        var import = GetXmlImport(args);
        if (!import.ErrorLog.HasErrors)
        {
            import.PersistImportToRepository();
            appCachePurger.PurgeApp(args.AppId);
        }

        return l.Return(new(!import.ErrorLog.HasErrors, null), "done, errors: " + import.ErrorLog.HasErrors);
    }

    private ImportListXml GetXmlImport(ContentImportArgsDto args)
    {
        var l = Log.Fn<ImportListXml>("get xml import " + args.DebugInfo);
        var contextLanguages = appsCatalog
            .Zone(_appReader.ZoneId)
            .LanguagesActive
            .Select(lng => lng.EnvironmentKey)
            .ToArray();

        using var contentSteam = new MemoryStream(Convert.FromBase64String(args.ContentBase64));
        var importer = importListXml.Value.Init(
            _appReader,
            args.ContentType,
            contentSteam,
            contextLanguages,
            args.DefaultLanguage,
            args.ClearEntities,
            args.ImportResourcesReferences,
            LogSettings
        );
        return l.Return(importer);
    }

    [HttpPost]
    public bool Import(EntityImportDto args)
    {
        var l = Log.Fn<bool>(message: "import json item" + args.DebugInfo);
        try
        {
            var deserializer = jsonSerializerLazy.Value.SetApp(_appReader);
            // Since we're importing directly into this app, we prefer local content-types
            deserializer.PreferLocalAppTypes = true;

            var listToImport = new List<IEntity> { deserializer.Deserialize(args.GetContentString()) };

            workEntSave.New(_appReader).Import(listToImport);

            return l.ReturnTrue();
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            l.Ex(ex);
            throw new("Couldn't import - probably bad file format", ex);
        }
    }
}
