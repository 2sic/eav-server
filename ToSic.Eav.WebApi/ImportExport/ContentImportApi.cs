using System.IO;
using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.State;
using ToSic.Eav.ImportExport.Internal.XmlList;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Serialization.Internal;

#if NETFRAMEWORK
using System.Web.Http;
#else
using Microsoft.AspNetCore.Mvc;
#endif

namespace ToSic.Eav.WebApi.ImportExport;

/// <inheritdoc />
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ContentImportApi(
    LazySvc<ImportListXml> importListXml,
    LazySvc<JsonSerializer> jsonSerializerLazy,
    AppCachePurger appCachePurger,
    GenWorkDb<WorkEntitySave> workEntSave,
    IAppStates appStates)
    : ServiceBase("Api.EaCtIm", connect: [workEntSave, importListXml, jsonSerializerLazy, appCachePurger, appStates])
{
    private IAppStateInternal _appState;

    public ContentImportApi Init(int appId)
    {
        var l = Log.Fn<ContentImportApi>($"app: {appId}");
        _appState = appStates.GetReader(appId);
        return l.Return(this);
    }


    [HttpPost]
    public ContentImportResultDto XmlPreview(ContentImportArgsDto args)
    {
        var l = Log.Fn<ContentImportResultDto>("eval content - start" + args.DebugInfo);

        var import = GetXmlImport(args);
        var result = import.ErrorLog.HasErrors
            ? new(!import.ErrorLog.HasErrors, import.ErrorLog.Errors)
            : new ContentImportResultDto(!import.ErrorLog.HasErrors, new ImportStatisticsDto
            {
                AmountOfEntitiesCreated = import.Info_AmountOfEntitiesCreated,
                AmountOfEntitiesDeleted = import.Info_AmountOfEntitiesDeleted,
                AmountOfEntitiesUpdated = import.Info_AmountOfEntitiesUpdated,
                AttributeNamesInDocument = import.Info_AttributeNamesInDocument,
                AttributeNamesInContentType = import.Info_AttributeNamesInContentType,
                AttributeNamesNotImported = import.Info_AttributeNamesNotImported,
                DocumentElementsCount = import.DocumentElements.Count(),
                LanguagesInDocumentCount = import.Info_LanguagesInDocument.Count()
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
        var contextLanguages = appStates.Languages(_appState.ZoneId).Select(lng => lng.EnvironmentKey).ToArray();

        using var contentSteam = new MemoryStream(Convert.FromBase64String(args.ContentBase64));
        var importer = importListXml.Value.Init(_appState, args.ContentType, contentSteam,
            contextLanguages, args.DefaultLanguage,
            args.ClearEntities, args.ImportResourcesReferences);
        return l.Return(importer);
    }

    [HttpPost]
    public bool Import(EntityImportDto args)
    {
        var l = Log.Fn<bool>(message: "import json item" + args.DebugInfo);
        try
        {
            var deserializer = jsonSerializerLazy.Value.SetApp(_appState);
            // Since we're importing directly into this app, we prefer local content-types
            deserializer.PreferLocalAppTypes = true;

            var listToImport = new List<IEntity> { deserializer.Deserialize(args.GetContentString()) };

            workEntSave.New(_appState).Import(listToImport);

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
