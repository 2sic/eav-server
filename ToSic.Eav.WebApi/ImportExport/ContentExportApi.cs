using System;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.ImportExport.Options;
using ToSic.Eav.ImportExport.Validation;
using ToSic.Eav.Metadata;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Plumbing;
using ToSic.Eav.WebApi.Plumbing;
using ToSic.Eav.WebApi.Security;
using System.Collections.Generic;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Lib.DI;
#if NETFRAMEWORK
using System.Web.Http;
#else
using Microsoft.AspNetCore.Mvc;
#endif

namespace ToSic.Eav.WebApi.ImportExport
{
    public class ContentExportApi<THttpResponseType> : HasLog
    {
        private AppManager _appManager;
        public ContentExportApi(
            Lazy<AppManager> appManagerLazy, 
            IAppStates appStates,
            Generator<JsonSerializer> jsonSerializer,
            ResponseMaker<THttpResponseType> responseMaker
            ) : base("Api.EaCtEx")
        {
            _appManagerLazy = appManagerLazy;
            _appStates = appStates;
            _jsonSerializer = jsonSerializer;
            _responseMaker = responseMaker;
        }
        private readonly Lazy<AppManager> _appManagerLazy;
        private readonly IAppStates _appStates;
        private readonly Generator<JsonSerializer> _jsonSerializer;
        private readonly ResponseMaker<THttpResponseType> _responseMaker;

        public ContentExportApi<THttpResponseType> Init(int appId, ILog parentLog)
        {
            this.Init(parentLog);
            _appManager = _appManagerLazy.Value.Init(appId, Log);
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
            string selectedIds)
        {
            Log.A($"export content lang:{language}, deflang:{defaultLanguage}, ct:{contentType}, ids:{selectedIds}");
            SecurityHelpers.ThrowIfNotAdmin(user.IsSiteAdmin);

            var contextLanguages = _appStates.Languages(_appManager.ZoneId).Select(l => l.EnvironmentKey).ToArray();

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
        }

        [HttpGet]
        public THttpResponseType DownloadTypeAsJson(IUser user, string name)
        {
            Log.A($"get fields type:{name}");
            SecurityHelpers.ThrowIfNotAdmin(user.IsSiteAdmin);
            var type = _appManager.Read.ContentTypes.Get(name);
            var serializer = _jsonSerializer.New().Init(_appManager.AppState, Log);
            var fileName = (type.Scope + "." + type.NameId + ImpExpConstants.Extension(ImpExpConstants.Files.json))
                .RemoveNonFilenameCharacters();
 
            return _responseMaker.File(serializer.Serialize(type), fileName, MimeHelper.Json);
        }

        [HttpGet]
        public THttpResponseType DownloadEntityAsJson(IUser user, int id, string prefix, bool withMetadata)
        {
            Log.A($"get fields id:{id}");
            SecurityHelpers.ThrowIfNotAdmin(user.IsSiteAdmin);
            var entity = _appManager.Read.Entities.Get(id);
            var serializer = _jsonSerializer.New().Init(_appManager.AppState, Log);

            return _responseMaker.File(
                serializer.Serialize(entity, withMetadata ? FileSystemLoader.QueryMetadataDepth : 0),
                (prefix + (string.IsNullOrWhiteSpace(prefix) ? "" : ".")
                 + entity.GetBestTitle() + ImpExpConstants.Extension(ImpExpConstants.Files.json))
                .RemoveNonFilenameCharacters());
        }
        
        [HttpGet]
        public THttpResponseType JsonBundleExport(IUser user, Guid exportConfiguration)
        {
            Log.A($"create Json Bundle Export for ExportConfiguration:{exportConfiguration}");
            //SecurityHelpers.ThrowIfNotAdmin(user.IsSiteAdmin); // TODO: uncomment this

            var systemExportConfiguration = _appManager.AppState.List.One(exportConfiguration);
            if (systemExportConfiguration == null)
            {
                var exception = new KeyNotFoundException($"ExportConfiguration:{exportConfiguration} is missing");
                Log.Ex(exception);
                throw exception;
            }

            // check that have correct contentType
            if (systemExportConfiguration.Type.Is("ExportConfiguration"))
            {
                var exception = new KeyNotFoundException($"ExportConfiguration:{exportConfiguration} is not of type ExportConfiguration");
                Log.Ex(exception);
                throw exception;
            }

            // TODO: implement PreserveMarkers functionality
            var preserveMarkers = systemExportConfiguration.GetBestValue<bool>("PreserveMarkers", null);
            Log.A($"preserveMarkers:{preserveMarkers}");

            // 1. Find all decorator metadata of type SystemExportDecorator
            // use the guid for finding them: 32698880-1c2e-41ab-bcfc-420091d3263f
            // filter by the Configuration field

            // TODO create type that is EntityBasedType


            var metadataExportMarkers = systemExportConfiguration.Parents(Decorators.SystemExportDecorator);
            Log.A($"metadataExportMarkers:{metadataExportMarkers.Count()}");


            // 2. From the metadata, find all owners
            // TODO: should be other way to get this selection (also this is not working for entities)
            var owners = metadataExportMarkers.Where(e => e.MetadataFor.TargetType == (int)TargetTypes.ContentType)
                .Select(et => et.MetadataFor.KeyString).ToList();
            Log.A($"count owners:{owners.Count()}");

            var serializer = _jsonSerializer.New().Init(_appManager.AppState, Log);

            // TODO: wrong JSON v1 format is generated
            var bundleList = new JsonBundle();

            // 3. Loop through content types and add them to the bundlelist
            var contentTypes = _appManager.Read.AppState.ContentTypes.Where(ct => owners.Contains(ct.NameId)).ToList();
            Log.A($"count export contentTypes:{contentTypes.Count()}");
            foreach (var contentType in contentTypes)
            {
                var jsonType = serializer.ToPackage(contentType, true);
                if (bundleList.ContentTypes == null) bundleList.ContentTypes = new List<JsonContentTypeSet>();
                bundleList.ContentTypes.Add(new JsonContentTypeSet
                {
                    ContentType = jsonType.ContentType,
                    Entities = jsonType.Entities
                });
            }

            // 4. loop through entities and add them to the bundle list
            var entities = _appManager.Read.AppState.List.Where(e => owners.Contains(e.EntityGuid.ToString()));
            Log.A($"count export entities:{entities.Count()}");
            foreach (var entity in entities)
            {
                if (bundleList.Entities == null) bundleList.Entities = new List<JsonEntity>();
                bundleList.Entities.Add(serializer.ToJson(entity));
            }

            // 5. Create a file which contains this new bundle
            var fileContent = System.Text.Json.JsonSerializer.Serialize(new JsonFormat
            {
                Bundles = new List<JsonBundle>() { bundleList }
            }, Serialization.JsonOptions.UnsafeJsonWithoutEncodingHtml);

            // 6. give it to the browser with the name specified in the Export Configuration
            var fileName = systemExportConfiguration.GetBestValue<string>("FileName", null);

            Log.A($"OK, export fileName:{fileName}, size:{fileContent.Count()}");
            return _responseMaker.File(fileContent, fileName, MimeHelper.Json);
        }
    }
}
