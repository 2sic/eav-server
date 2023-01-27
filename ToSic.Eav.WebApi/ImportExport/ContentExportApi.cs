using System;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.ImportExport.Options;
using ToSic.Eav.ImportExport.Validation;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Plumbing;
using ToSic.Eav.WebApi.Plumbing;
using ToSic.Eav.WebApi.Security;
using System.Collections.Generic;
using ToSic.Eav.Configuration;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Serialization;
using ToSic.Lib.DI;
using ToSic.Lib.Services;
#if NETFRAMEWORK
using System.Web.Http;
#else
using Microsoft.AspNetCore.Mvc;
#endif

namespace ToSic.Eav.WebApi.ImportExport
{
    public class ContentExportApi<THttpResponseType> : ServiceBase
    {
        private readonly LazySvc<AppManager> _appManagerLazy;
        private readonly IAppStates _appStates;
        private readonly Generator<JsonSerializer> _jsonSerializer;
        private readonly ResponseMaker<THttpResponseType> _responseMaker;
        private readonly LazySvc<IFeaturesInternal> _features;

        private AppManager _appManager;
        public ContentExportApi(
            LazySvc<AppManager> appManagerLazy, 
            IAppStates appStates,
            Generator<JsonSerializer> jsonSerializer,
            ResponseMaker<THttpResponseType> responseMaker,
            LazySvc<IFeaturesInternal> features
            ) : base("Api.EaCtEx")
        {

            ConnectServices(
                _appManagerLazy = appManagerLazy,
                _appStates = appStates,
                _jsonSerializer = jsonSerializer,
                _responseMaker = responseMaker,
                _features = features
            );
        }

        public ContentExportApi<THttpResponseType> Init(int appId)
        {
            _appManager = _appManagerLazy.Value.Init(appId);
            Log.A($"For app: {appId}");
            return this;
        }

        public Tuple<string, string> ExportContent(IUser user,
            string language,
            string defaultLanguage,
            string contentType,
            ExportSelection exportSelection,
            ExportResourceReferenceMode exportResourcesReferences,
            ExportLanguageResolution exportLanguageReferences,
            string selectedIds) => Log.Func(l =>
        {
            l.A($"export content lang:{language}, deflang:{defaultLanguage}, ct:{contentType}, ids:{selectedIds}");
            SecurityHelpers.ThrowIfNotAdmin(user.IsSiteAdmin);

            var contextLanguages = _appStates.Languages(_appManager.ZoneId).Select(lng => lng.EnvironmentKey).ToArray();

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

            var tableExporter = _appManager.Entities.Exporter(contentType);
            var fileContent = exportSelection == ExportSelection.Blank
                ? tableExporter.EmptyListTemplate()
                : tableExporter.GenerateXml(language ?? "", defaultLanguage, contextLanguages, exportLanguageReferences,
                    exportResourcesReferences == ExportResourceReferenceMode.Resolve, ids);

            var contentTypeName = tableExporter.ContentType.Name;

            var fileName =
                $"2sxc {contentTypeName.Replace(" ", "-")} {language} " +
                $"{(exportSelection == ExportSelection.Blank ? "Template" : "Data")} " +
                $"{DateTime.Now:yyyyMMddHHmmss}.xml";

            return new Tuple<string, string>(fileContent, fileName);
        });

        [HttpGet]
        public THttpResponseType DownloadTypeAsJson(IUser user, string name) => Log.Func(l =>
        {
            l.A($"get fields type:{name}");
            SecurityHelpers.ThrowIfNotAdmin(user.IsSiteAdmin);
            var type = _appManager.Read.ContentTypes.Get(name);
            var serializer = _jsonSerializer.New().SetApp(_appManager.AppState);
            var fileName = (type.Scope + "." + type.NameId + ImpExpConstants.Extension(ImpExpConstants.Files.json))
                .RemoveNonFilenameCharacters();

            return _responseMaker.File(serializer.Serialize(type), fileName, MimeHelper.Json);
        });

        [HttpGet]
        public THttpResponseType DownloadEntityAsJson(IUser user, int id, string prefix, bool withMetadata) => Log.Func(l =>
        {
            l.A($"get fields id:{id}");
            SecurityHelpers.ThrowIfNotAdmin(user.IsSiteAdmin);
            var entity = _appManager.Read.Entities.Get(id);
            var serializer = _jsonSerializer.New().SetApp(_appManager.AppState);

            return _responseMaker.File(
                serializer.Serialize(entity, withMetadata ? FileSystemLoader.QueryMetadataDepth : 0),
                (prefix + (string.IsNullOrWhiteSpace(prefix) ? "" : ".")
                 + entity.GetBestTitle() + ImpExpConstants.Extension(ImpExpConstants.Files.json))
                .RemoveNonFilenameCharacters());
        });

        [HttpGet]
        public THttpResponseType JsonBundleExport(IUser user, Guid exportConfiguration, int indentation) => Log.Func(l =>
        {
            l.A($"create Json Bundle Export for ExportConfiguration:{exportConfiguration}");
            SecurityHelpers.ThrowIfNotAdmin(user.IsSiteAdmin);

            _features.Value.ThrowIfNotEnabled("This feature is required", BuiltInFeatures.DataExportImportBundles.Guid);

            var export = ExportConfigurationBuildOrThrow(exportConfiguration);

            // find all decorator metadata of type SystemExportDecorator
            l.A($"metadataExportMarkers:{export.ExportMarkers.Count}");

            var serializer = _jsonSerializer.New().SetApp(_appManager.AppState);

            var bundle = BundleBuild(export, serializer);

            // create a file which contains this new bundle
            var fileContent = serializer.SerializeJsonBundle(bundle, indentation);

            // give it to the browser with the name specified in the Export Configuration
            l.A($"OK, export fileName:{export.FileName}, size:{fileContent.Count()}");
            return _responseMaker.File(fileContent, export.FileName, MimeHelper.Json);
        });
        
        public ExportConfiguration ExportConfigurationBuildOrThrow(Guid exportConfiguration) => Log.Func(l =>
        {
            var systemExportConfiguration = _appManager.AppState.List.One(exportConfiguration);
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
            
            return new ExportConfiguration(systemExportConfiguration);
        });

        private JsonBundle BundleBuild(ExportConfiguration export, JsonSerializer serializer) => Log.Func(l =>
        {
            var bundleList = new JsonBundle();

            // loop through content types and add them to the bundlelist
            l.A($"count export content types:{export.ContentTypes.Count}");
            foreach (var contentTypeName in export.ContentTypes)
            {
                if (bundleList.ContentTypes == null) bundleList.ContentTypes = new List<JsonContentTypeSet>();

                var contentType = _appManager.Read.ContentTypes.Get(contentTypeName);
                var jsonType = serializer.ToPackage(contentType, true);
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

                var entity = _appManager.Read.Entities.Get(entityGuid);
                bundleList.Entities.Add(serializer.ToJson(entity, export.EntitiesWithMetadata ? FileSystemLoader.QueryMetadataDepth : 0));
            }

            return bundleList;
        });

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
}
