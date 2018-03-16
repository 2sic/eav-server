using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.ImportExport.Options;
using ToSic.Eav.ImportExport.Validation;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.WebApi.Helpers;

namespace ToSic.Eav.WebApi
{
    public class ContentExportController : Eav3WebApiBase
    {
        public ContentExportController(Log parentLog = null) : base(parentLog)
        {
            Log.Rename("EaCtEx");
        }

        [HttpGet]
        public HttpResponseMessage ExportContent(int appId, string language, string defaultLanguage, string contentType,
            ExportSelection exportSelection, ExportResourceReferenceMode exportResourcesReferences,
            ExportLanguageResolution exportLanguageReferences, string selectedIds = null) 
            => ExportContentNew(appId, language, defaultLanguage, contentType, exportSelection, exportResourcesReferences, exportLanguageReferences, selectedIds);

        private HttpResponseMessage ExportContentNew(int appId, string language, string defaultLanguage, string contentType,
    ExportSelection exportSelection, ExportResourceReferenceMode exportResourcesReferences,
    ExportLanguageResolution exportLanguageReferences, string selectedIds)
        {
            Log.Add(
                $"export content NEW a#{appId}, lang:{language}, deflang:{defaultLanguage}, ct:{contentType}, ids:{selectedIds}");
            AppId = appId;

            var contextLanguages = AppManager.Read.Zone.Languages().Select(l => l.EnvironmentKey).ToArray();

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

            var tableExporter = AppManager.Entities.Exporter(contentType);
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
            SetAppId(appId);

            var type = AppManager.Read.ContentTypes.Get(name);
            var serializer = new JsonSerializer(AppManager.Package, Log);

            return Download.BuildDownload(serializer.Serialize(type),
                (type.Scope + "." + type.StaticName + ImpExpConstants.Extension(ImpExpConstants.Files.json))
                     .RemoveNonFilenameCharacters());
        }

        [HttpGet]
        public HttpResponseMessage DownloadEntityAsJson(int appId, int id, string prefix, bool withMetadata)
        {
            Log.Add($"get fields a#{appId}, id:{id}");
            SetAppId(appId);

            var entity = AppManager.Read.Entities.Get(id);
            var serializer = new JsonSerializer(AppManager.Package, Log);

            return Download.BuildDownload(
                serializer.Serialize(entity, withMetadata ? FileSystemLoader.QueryMetadataDepth : 0),
                (prefix + (string.IsNullOrWhiteSpace(prefix) ? "" : ".")
                 + entity.GetBestTitle() + ImpExpConstants.Extension(ImpExpConstants.Files.json))
                .RemoveNonFilenameCharacters());
        }

    }
}
