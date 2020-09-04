using System;
using System.Linq;
using System.Net.Http;
#if NET451
using System.Web.Http;
#else
using Microsoft.AspNetCore.Mvc;
#endif
using ToSic.Eav.Apps;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.ImportExport.Options;
using ToSic.Eav.ImportExport.Validation;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.WebApi.Helpers;

namespace ToSic.Eav.WebApi
{
    public class ContentExportApi : HasLog
    {
        public ContentExportApi(ILog parentLog = null) : base("Api.EaCtEx", parentLog)
        {
        }

        public HttpResponseMessage ExportContent(int appId, string language, string defaultLanguage, string contentType,
            ExportSelection exportSelection, ExportResourceReferenceMode exportResourcesReferences,
            ExportLanguageResolution exportLanguageReferences, string selectedIds = null) 
            => ExportContentNew(appId, language, defaultLanguage, contentType, exportSelection, exportResourcesReferences, exportLanguageReferences, selectedIds);

        private HttpResponseMessage ExportContentNew(int appId, string language, 
            string defaultLanguage, 
            string contentType,
            ExportSelection exportSelection, 
            ExportResourceReferenceMode exportResourcesReferences,
            ExportLanguageResolution exportLanguageReferences, 
            string selectedIds)
        {
            Log.Add($"export content NEW a#{appId}, lang:{language}, " +
                    $"deflang:{defaultLanguage}, ct:{contentType}, ids:{selectedIds}");
            var appManager = new AppManager(appId, Log);
            var contextLanguages = appManager.Read.Zone.Languages().Select(l => l.EnvironmentKey).ToArray();

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

            var tableExporter = appManager.Entities.Exporter(contentType);
            var fileContent = exportSelection == ExportSelection.Blank
                ? tableExporter.EmptyListTemplate()
                : tableExporter.GenerateXml(language ?? "", defaultLanguage, contextLanguages, exportLanguageReferences,
                    exportResourcesReferences == ExportResourceReferenceMode.Resolve, ids);

            var contentTypeName = tableExporter.ContentType.Name;


            var fileName =
                $"2sxc {contentTypeName.Replace(" ", "-")} {language} " +
                $"{(exportSelection == ExportSelection.Blank ? "Template" : "Data")} " +
                $"{DateTime.Now:yyyyMMddHHmmss}.xml";

            return Download.BuildDownload(fileContent, fileName);

        }

        [HttpGet]
        public HttpResponseMessage DownloadTypeAsJson(int appId, string name)
        {
            Log.Add($"get fields a#{appId}, type:{name}");
            var appManager = new AppManager(appId, Log);
            var type = appManager.Read.ContentTypes.Get(name);
            var serializer = new JsonSerializer(appManager.AppState, Log);

            return Download.BuildDownload(serializer.Serialize(type),
                (type.Scope + "." + type.StaticName + ImpExpConstants.Extension(ImpExpConstants.Files.json))
                     .RemoveNonFilenameCharacters());
        }

        [HttpGet]
        public HttpResponseMessage DownloadEntityAsJson(int appId, int id, string prefix, bool withMetadata)
        {
            Log.Add($"get fields a#{appId}, id:{id}");
            var appManager = new AppManager(appId, Log);
            var entity = appManager.Read.Entities.Get(id);
            var serializer = new JsonSerializer(appManager.AppState, Log);

            return Download.BuildDownload(
                serializer.Serialize(entity, withMetadata ? FileSystemLoader.QueryMetadataDepth : 0),
                (prefix + (string.IsNullOrWhiteSpace(prefix) ? "" : ".")
                 + entity.GetBestTitle() + ImpExpConstants.Extension(ImpExpConstants.Files.json))
                .RemoveNonFilenameCharacters());
        }

    }
}
