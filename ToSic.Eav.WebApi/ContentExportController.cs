using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using ToSic.Eav.ImportExport.Options;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Repository.Efc.Parts;

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
        {
            // return ExportContentOld(appId, language, defaultLanguage, contentType, exportSelection, exportResourcesReferences, exportLanguageReferences, selectedIds);
            return ExportContentNew(appId, language, defaultLanguage, contentType, exportSelection, exportResourcesReferences, exportLanguageReferences, selectedIds);
        }

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

            //var tableExporter = new DbXmlExportTable(CurrentContext);
            //tableExporter.Init(contentTypeId);
            var tableExporter = AppManager.Entities.Exporter(contentType);
            var fileContent = exportSelection == ExportSelection.Blank
                ? tableExporter.EmptySchemaXml()
                : tableExporter.GenerateXml(language ?? "", defaultLanguage, contextLanguages, exportLanguageReferences,
                    exportResourcesReferences, ids);

            var contentTypeName = tableExporter.ContentType.Name; // 2017-04-23 2dm check! ct.Name;


            var fileName =
                $"2sxc {contentTypeName.Replace(" ", "-")} {language} " +
                $"{(exportSelection == ExportSelection.Blank ? "Template" : "Data")} " +
                $"{DateTime.Now:yyyyMMddHHmmss}.xml";

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(fileContent)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };
            return response;
        }

        private HttpResponseMessage ExportContentOld(int appId, string language, string defaultLanguage, string contentType,
            ExportSelection exportSelection, ExportResourceReferenceMode exportResourcesReferences,
            ExportLanguageResolution exportLanguageReferences, string selectedIds)
        {
            Log.Add(
                $"export content OLD a#{appId}, lang:{language}, deflang:{defaultLanguage}, ct:{contentType}, ids:{selectedIds}");
            AppId = appId;

            // todo: continue here!
            //var ct = CurrentContext.AttribSet.GetAttributeSetWithEitherName(contentType);

            var contentTypeId = CurrentContext.AttribSet.GetIdWithEitherName(contentType);
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

            var tableExporter = new DbXmlExportTable(CurrentContext);
            tableExporter.Init(contentTypeId);
            var fileContent = exportSelection == ExportSelection.Blank
                ? tableExporter.EmptySchemaXml()
                : tableExporter.TableXmlFromDb(language ?? "", defaultLanguage, contextLanguages, exportLanguageReferences,
                    exportResourcesReferences, ids);

            var contentTypeName = tableExporter.NiceContentTypeName; // 2017-04-23 2dm check! ct.Name;


            var fileName =
                $"2sxc {contentTypeName.Replace(" ", "-")} {language} {(exportSelection == ExportSelection.Blank ? "Template" : "Data")} {DateTime.Now:yyyyMMddHHmmss}.xml";

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(fileContent)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };
            return response;
        }
    }
}
