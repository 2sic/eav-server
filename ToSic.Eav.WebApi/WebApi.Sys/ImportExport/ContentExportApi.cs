using ToSic.Eav.Apps.Sys.Paths;
using ToSic.Eav.Data.Entities.Sys.Lists;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Security;
using ToSic.Eav.WebApi.Infrastructure;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.ImportExport.Sys;
using ToSic.Eav.ImportExport.Sys.Options;
using ToSic.Eav.ImportExport.Sys.XmlList;
using ToSic.Eav.Internal;
using ToSic.Eav.Security.Files;
using ToSic.Eav.Serialization.Sys;
using ToSic.Eav.Sys;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Capabilities.SysFeatures;
using ToSic.Sys.Users;
#if NETFRAMEWORK
using System.Web.Http;
using System.Web.UI.WebControls;
#else
using Microsoft.AspNetCore.Mvc;
#endif
#if NETFRAMEWORK
using THttpResponseType = System.Net.Http.HttpResponseMessage;
#else
using THttpResponseType = Microsoft.AspNetCore.Mvc.IActionResult;
#endif


namespace ToSic.Eav.WebApi.ImportExport;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ContentExportApi(
    IAppPathsMicroSvc appPathSvc,
    AppWorkContextService appWorkCtxSvc,
    IAppsCatalog appsCatalog,
    Generator<JsonSerializer> jsonSerializer,
    IResponseMaker responseMaker,
    Generator<ExportListXml> exportListXmlGenerator,
    LazySvc<ISysFeaturesService> features)
    : ServiceBase("Api.EaCtEx",
        connect: [appPathSvc, appWorkCtxSvc, exportListXmlGenerator, appsCatalog, jsonSerializer, responseMaker, features])
{
    public ContentExportApi Init(int appId)
    {
        var l = Log.Fn<ContentExportApi>($"For app: {appId}");
        _appCtx = appWorkCtxSvc.Context(appId);
        return l.Return(this);
    }

    private IAppWorkCtx _appCtx;

    public (string FileContents, string FileName) ExportContent(IUser user,
        string language,
        string defaultLanguage,
        string contentType,
        ExportSelection exportSelection,
        ExportResourceReferenceMode exportResourcesReferences,
        ExportLanguageResolution exportLanguageReferences,
        string selectedIds)
    {
        var l = Log.Fn<(string, string)>($"export content lang:{language}, deflang:{defaultLanguage}, ct:{contentType}, ids:{selectedIds}");
        SecurityHelpers.ThrowIfNotContentAdmin(user, l);

        var contextLanguages = appsCatalog.Zone(_appCtx.ZoneId).LanguagesActive
            .Select(lng => lng.EnvironmentKey)
            .ToArray();

        // check if we have an array of ids
        int[] ids = null;
        try
        {
            if (exportSelection == ExportSelection.Selection && !string.IsNullOrWhiteSpace(selectedIds))
                ids = selectedIds.Split(',').Select(int.Parse).ToArray();
        }
        catch (Exception e)
        {
            throw new("trouble finding selected IDs to export", e);
        }

        var tableExporter = exportListXmlGenerator.New().Init(_appCtx.AppReader, contentType);
        var fileContent = exportSelection == ExportSelection.Blank
            ? tableExporter.EmptyListTemplate()
            : tableExporter.GenerateXml(language ?? "", defaultLanguage, contextLanguages, exportLanguageReferences,
                exportResourcesReferences == ExportResourceReferenceMode.Resolve, ids);

        var contentTypeName = tableExporter.ContentType.Name;

        var fileName =
            $"2sxc {contentTypeName.Replace(" ", "-")} {language} " +
            $"{(exportSelection == ExportSelection.Blank ? "Template" : "Data")} " +
            $"{DateTime.Now:yyyyMMddHHmmss}.xml";

        return l.ReturnAsOk((fileContent, fileName));
    }

    [HttpGet]
    public THttpResponseType DownloadTypeAsJson(IUser user, string name)
    {
        var l = Log.Fn<THttpResponseType>($"get fields type:{name}");
        SecurityHelpers.ThrowIfNotSiteAdmin(user, l);
        var type = _appCtx.AppReader.GetContentType(name);
        var serializer = jsonSerializer.New().SetApp(_appCtx.AppReader);
        var fileName = (type.Scope + "." + type.NameId + ImpExpConstants.Extension(ImpExpConstants.Files.json))
            .RemoveNonFilenameCharacters();

        var typeJson = serializer.Serialize(type, new()
        {
            CtIncludeInherited = false,
            CtAttributeIncludeInheritedMetadata = false
        });
        return l.ReturnAsOk(responseMaker.File(typeJson, fileName, MimeTypeConstants.Json));
    }

    [HttpGet]
    public THttpResponseType DownloadEntityAsJson(IUser user, int id, string prefix, bool withMetadata)
    {
        var l = Log.Fn<THttpResponseType>($"get fields id:{id}");
        SecurityHelpers.ThrowIfNotSiteAdmin(user, l);
        var entity = _appCtx.AppReader.List.FindRepoId(id);
        var serializer = jsonSerializer.New().SetApp(_appCtx.AppReader);

        return l.ReturnAsOk(responseMaker.File(
            serializer.Serialize(entity, withMetadata ? FileSystemLoaderConstants.QueryMetadataDepth : 0),
            (prefix + (string.IsNullOrWhiteSpace(prefix) ? "" : ".")
             + entity.GetBestTitle() + ImpExpConstants.Extension(ImpExpConstants.Files.json))
            .RemoveNonFilenameCharacters()));
    }

    [HttpGet]
    public THttpResponseType JsonBundleExport(IUser user, Guid exportConfiguration, int indentation)
    {
        var l = Log.Fn<THttpResponseType>($"create Json Bundle Export for ExportConfiguration:{exportConfiguration}");
        SecurityHelpers.ThrowIfNotSiteAdmin(user, l);

        features.Value.ThrowIfNotEnabled("This feature is required", BuiltInFeatures.DataExportImportBundles.Guid);

        var (export, fileContent) = CreateBundleExport(exportConfiguration, indentation);

        return l.ReturnAsOk(responseMaker.File(fileContent, export.FileName, MimeTypeConstants.Json));
    }

    public bool BundleSave(IUser user, Guid exportConfiguration, int indentation)
    {
        var l = Log.Fn<bool>($"for ExportConfiguration:{exportConfiguration}");

        SecurityHelpers.ThrowIfNotSiteAdmin(user, l);

        features.Value.ThrowIfNotEnabled("This feature is required", BuiltInFeatures.DataExportImportBundles.Guid);

        var (export, fileContent) = CreateBundleExport(exportConfiguration, indentation);

        var appPaths = appPathSvc.Get(_appCtx.AppReader);

        var appDataPath = Path.Combine(appPaths.PhysicalPath, FolderConstants.AppDataProtectedFolder, AppDataFoldersConstants.BundlesFolder);
        l.A($"appDataPath:'{appDataPath}'");

        try
        {
            // create App_Data\bundles unless exists
            Directory.CreateDirectory(appDataPath);

            var fileNameSafe = FileNames.SanitizeFileName(export.FileName);
            if (export.FileName != fileNameSafe) l.A($"File name sanitized:'{export.FileName}' => '{fileNameSafe}'");

            var filePath = Path.Combine(appDataPath, fileNameSafe);

            // save bundle file to App_Data\bundles
            File.WriteAllText(filePath, fileContent);
            l.A($"bundle saved, filePath:'{filePath}',size:{fileContent.Length}");

            return l.ReturnTrue();
        }
        catch (Exception ex)
        {
            l.Ex(ex);
            return l.ReturnFalse();
        }
    }

    private (ExportConfiguration export, string fileContent) CreateBundleExport(Guid exportConfiguration, int indentation)
    {
        var l = Log.Fn<(ExportConfiguration export, string fileContent)>($"create bundle export for ExportConfiguration:{exportConfiguration}");

        var export = ExportConfigurationBuildOrThrow(exportConfiguration);

        // find all decorator metadata of type SystemExportDecorator
        l.A($"metadataExportMarkers:{export.ExportMarkers.Count}");

        var serializer = jsonSerializer.New().SetApp(_appCtx.AppReader);

        var bundle = BundleBuild(export, serializer);

        // create a file which contains this new bundle
        var fileContent = serializer.SerializeJsonBundle(bundle, indentation);

        // give it to the browser with the name specified in the Export Configuration
        l.A($"OK, export fileName:{export.FileName}, size:{fileContent.Length}");

        return l.ReturnAsOk((export, fileContent));
    }

    private ExportConfiguration ExportConfigurationBuildOrThrow(Guid exportConfiguration)
    {
        var l = Log.Fn<ExportConfiguration>($"build ExportConfiguration:{exportConfiguration}");
        var systemExportConfiguration = _appCtx.AppReader.List.One(exportConfiguration);
        if (systemExportConfiguration == null)
        {
            var exception = new KeyNotFoundException($"ExportConfiguration:{exportConfiguration} is missing");
            l.Ex(exception);
            throw exception;
        }

        // check that have correct contentType
        if (systemExportConfiguration.Type.Is("ExportConfiguration"))
        {
            var exception =
                new KeyNotFoundException($"ExportConfiguration:{exportConfiguration} is not of type ExportConfiguration");
            l.Ex(exception);
            throw exception;
        }
        
        return l.ReturnAsOk(new(systemExportConfiguration));
    }

    private JsonBundle BundleBuild(ExportConfiguration export, JsonSerializer serializer)
    {
        var l = Log.Fn<JsonBundle>($"build bundle for ExportConfiguration:{export.Guid}");
        var bundleList = new JsonBundle();

        // loop through content types and add them to the bundle-list
        l.A($"count export content types:{export.ContentTypes.Count}");
        var serSettings = new JsonSerializationSettings
        {
            CtIncludeInherited = true,
            CtAttributeIncludeInheritedMetadata = false
        };
        var appState = _appCtx.AppReader;

        // Content-Types contains the Content-Type as well entities referenced in CT-Attribute Metadata such as Formulas
        bundleList.ContentTypes = export.ContentTypes.Count <= 0
            ? null
            : export.ContentTypes
                .Select(appState.GetContentType)
                .Select(ct => serializer.ToPackage(ct, serSettings))
                .Select(jsonType => new JsonContentTypeSet
                {
                    ContentType = PreserveMarker(export.PreserveMarkers, jsonType.ContentType),
                    Entities = jsonType.Entities
                })
                .ToList();

        // loop through entities and add them to the bundle list
        l.A($"count export entities:{export.Entities.Count}");

        bundleList.Entities = export.Entities.Count <= 0
            ? null
            : export.Entities
                .Select(appState.List.One)
                .Select(e => serializer.ToJson(e, export.EntitiesWithMetadata ? FileSystemLoaderConstants.QueryMetadataDepth : 0))
                .ToList();

        // Find duplicate related entities
        // as there are various ways they can appear, but we really only need them once
        var dupEntities = (bundleList.ContentTypes ?? [])
            .SelectMany(ct => ct.Entities.Select(e => new { Entity = e, Type = ct, List = ct.Entities }))
            .Concat((bundleList.Entities ?? []).Select(e => new { Entity = e, Type = null as JsonContentTypeSet, List = bundleList.Entities }))
            .GroupBy(e => e.Entity.Id)
            .Where(g => g.Count() > 1)
            .ToList();

        l.A($"Found {dupEntities.Count} duplicate entities in export.");

        var removes = dupEntities
            .Select(dupEntity =>
            {
                var keep = dupEntity.FirstOrDefault(e => e.Type != null) ?? dupEntity.First();
                return dupEntity.Where(e => e != keep).ToList();
            })
            .SelectMany(g => g)
            .ToList();

        foreach (var remove in removes)
            remove.List.Remove(remove.Entity);

        return l.ReturnAsOk(bundleList);
    }

    public JsonContentType PreserveMarker(bool preserveMarkers, JsonContentType jsonContentType)
    {
        Log.A($"preserveMarkers:{preserveMarkers}");
        if (preserveMarkers) return jsonContentType;
        
        var removeQue = jsonContentType.Metadata
            .Where(metaData => metaData.Type.Name == ExportDecorator.ContentTypeName).ToList();

        foreach (var item in removeQue)
            jsonContentType.Metadata.Remove(item);
        
        return jsonContentType;
    }
}
