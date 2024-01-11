using System;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.ImportExport.Validation;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Plumbing;
using System.Collections.Generic;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Data;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Security;
using ToSic.Eav.WebApi.Infrastructure;
using ToSic.Lib.DI;
using ToSic.Lib.Services;
using ToSic.Eav.Apps.Internal.Work;
using ToSic.Eav.ImportExport.Internal;
using ToSic.Eav.ImportExport.Internal.Options;
using ToSic.Eav.ImportExport.Internal.XmlList;
using ToSic.Eav.Serialization.Internal;
#if NETFRAMEWORK
using System.Web.Http;
#else
using Microsoft.AspNetCore.Mvc;
#endif
#if NETFRAMEWORK
using THttpResponseType = System.Net.Http.HttpResponseMessage;
#else
using THttpResponseType = Microsoft.AspNetCore.Mvc.IActionResult;
#endif


namespace ToSic.Eav.WebApi.ImportExport;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ContentExportApi : ServiceBase
{
    private readonly AppWorkContextService _appWorkCtxSvc;
    private readonly Generator<ExportListXml> _exportListXmlGenerator;
    private readonly IAppStates _appStates;
    private readonly Generator<JsonSerializer> _jsonSerializer;
    private readonly IResponseMaker _responseMaker;
    private readonly LazySvc<IEavFeaturesService> _features;

    public ContentExportApi(
        AppWorkContextService appWorkCtxSvc,
        IAppStates appStates,
        Generator<JsonSerializer> jsonSerializer,
        IResponseMaker responseMaker,
        Generator<ExportListXml> exportListXmlGenerator,
        LazySvc<IEavFeaturesService> features
        ) : base("Api.EaCtEx")
    {
        ConnectServices(
            _appWorkCtxSvc = appWorkCtxSvc,
            _exportListXmlGenerator = exportListXmlGenerator,
            _appStates = appStates,
            _jsonSerializer = jsonSerializer,
            _responseMaker = responseMaker,
            _features = features
        );
    }

    public ContentExportApi Init(int appId)
    {
        var l = Log.Fn<ContentExportApi>($"For app: {appId}");
        _appCtx = _appWorkCtxSvc.Context(appId);
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

        var contextLanguages = _appStates.Languages(_appCtx.ZoneId).Select(lng => lng.EnvironmentKey).ToArray();

        // check if we have an array of ids
        int[] ids = null;
        try
        {
            if (exportSelection == ExportSelection.Selection && !string.IsNullOrWhiteSpace(selectedIds))
                ids = selectedIds.Split(',').Select(int.Parse).ToArray();
        }
        catch (Exception e)
        {
            throw new Exception("trouble finding selected IDs to export", e);
        }

        var tableExporter = _exportListXmlGenerator.New().Init(_appCtx.AppState, contentType);
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
        var type = _appCtx.AppState.GetContentType(name);
        var serializer = _jsonSerializer.New().SetApp(_appCtx.AppState);
        var fileName = (type.Scope + "." + type.NameId + ImpExpConstants.Extension(ImpExpConstants.Files.json))
            .RemoveNonFilenameCharacters();

        var typeJson = serializer.Serialize(type, new JsonSerializationSettings
        {
            CtIncludeInherited = false,
            CtAttributeIncludeInheritedMetadata = false
        });
        return l.ReturnAsOk(_responseMaker.File(typeJson, fileName, MimeHelper.Json));
    }

    [HttpGet]
    public THttpResponseType DownloadEntityAsJson(IUser user, int id, string prefix, bool withMetadata)
    {
        var l = Log.Fn<THttpResponseType>($"get fields id:{id}");
        SecurityHelpers.ThrowIfNotSiteAdmin(user, l);
        var entity = _appCtx.AppState.List.FindRepoId(id);
        var serializer = _jsonSerializer.New().SetApp(_appCtx.AppState);

        return l.ReturnAsOk(_responseMaker.File(
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

        _features.Value.ThrowIfNotEnabled("This feature is required", BuiltInFeatures.DataExportImportBundles.Guid);

        var export = ExportConfigurationBuildOrThrow(exportConfiguration);

        // find all decorator metadata of type SystemExportDecorator
        l.A($"metadataExportMarkers:{export.ExportMarkers.Count}");

        var serializer = _jsonSerializer.New().SetApp(_appCtx.AppState);

        var bundle = BundleBuild(export, serializer);

        // create a file which contains this new bundle
        var fileContent = serializer.SerializeJsonBundle(bundle, indentation);

        // give it to the browser with the name specified in the Export Configuration
        l.A($"OK, export fileName:{export.FileName}, size:{fileContent.Count()}");
        return l.ReturnAsOk(_responseMaker.File(fileContent, export.FileName, MimeHelper.Json));
    }
    
    public ExportConfiguration ExportConfigurationBuildOrThrow(Guid exportConfiguration)
    {
        var l = Log.Fn<ExportConfiguration>($"build ExportConfiguration:{exportConfiguration}");
        var systemExportConfiguration = _appCtx.AppState.List.One(exportConfiguration);
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
        
        return l.ReturnAsOk(new ExportConfiguration(systemExportConfiguration));
    }

    private JsonBundle BundleBuild(ExportConfiguration export, JsonSerializer serializer)
    {
        var l = Log.Fn<JsonBundle>($"build bundle for ExportConfiguration:{export.Guid}");
        var bundleList = new JsonBundle();

        // loop through content types and add them to the bundlelist
        l.A($"count export content types:{export.ContentTypes.Count}");
        var serSettings = new JsonSerializationSettings
        {
            CtIncludeInherited = true,
            CtAttributeIncludeInheritedMetadata = false
        };
        var appState = _appCtx.AppState;
        foreach (var contentTypeName in export.ContentTypes)
        {
            if (bundleList.ContentTypes == null) bundleList.ContentTypes = new List<JsonContentTypeSet>();

            var contentType = appState.GetContentType(contentTypeName);
            var jsonType = serializer.ToPackage(contentType, serSettings);
            bundleList.ContentTypes.Add(new JsonContentTypeSet
            {
                ContentType = PreserveMarker(export.PreserveMarkers, jsonType.ContentType),
                Entities = jsonType.Entities
            });
        }

        // loop through entities and add them to the bundle list
        l.A($"count export entities:{export.Entities.Count}");
        foreach (var entityGuid in export.Entities)
        {
            if (bundleList.Entities == null) bundleList.Entities = new List<JsonEntity>();

            var entity = appState.List.One(entityGuid);
            bundleList.Entities.Add(serializer.ToJson(entity, export.EntitiesWithMetadata ? FileSystemLoaderConstants.QueryMetadataDepth : 0));
        }

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
