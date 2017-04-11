using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using ToSic.Eav.BLL.Parts;
using ToSic.Eav.ImportExport.Refactoring;
using ToSic.Eav.ImportExport.Refactoring.Options;
using ToSic.Eav.ImportExport.Xml;

namespace ToSic.Eav.WebApi
{
    public class ContentExportController : Eav3WebApiBase
    {

        [HttpGet]
        public HttpResponseMessage ExportContent(int appId, string language, string defaultLanguage, string contentType,
            RecordExport recordExport, ResourceReferenceExport resourcesReferences,
            LanguageReferenceExport languageReferences, string selectedIds = null)
        {
            AppId = appId;

            // todo: continue here!
            var ct = CurrentContext.AttribSet.GetAttributeSetWithEitherName(contentType);
            var contentTypeId = ct.AttributeSetID;// GetContentTypeId(contentType);
            var contentTypeName = ct.Name;// GetContentTypeName(contentType);
            var contextLanguages = GetContextLanguages();

            // check if we have an array of ids
            int[] ids = null;
            try
            {
                if (recordExport == RecordExport.Selection && !string.IsNullOrWhiteSpace(selectedIds))
                    ids = selectedIds.Split(',').Select(int.Parse).ToArray();
            }
            catch (Exception e)
            {
                throw new Exception("trouble finding selected IDs to export", e);
            }

            var tableExporter = new DbXmlExportTable(CurrentContext);
            var fileContent = recordExport == RecordExport.Blank
                ? /*new DbXmlExportTable().*/ tableExporter.ContentTypeSchemaXmlFromDb(/*CurrentContext.ZoneId, appId,*/ contentTypeId) 
                : /*new DbXmlExportTable().*/ tableExporter.ContentTypeXmlFromDb(/*CurrentContext.ZoneId, appId, */contentTypeId, language ?? "", defaultLanguage, contextLanguages, languageReferences, resourcesReferences, ids);

            var fileName = $"2sxc {contentTypeName.Replace(" ", "-")} {language} {(recordExport == RecordExport.Blank ? "Template" : "Data")} {DateTime.Now.ToString("yyyyMMddHHmmss")}.xml";

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(fileContent);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
            // 2rm 2016-02-27 removed, probably caused truncating issues
            //response.Content.Headers.ContentLength = fileContent.Length;
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };
            return response;
        }

        private string[] GetContextLanguages()
            => /*AppManager.Read.*/ CurrentContext.Dimensions.GetLanguages().Select(language => language.ExternalKey).ToArray();

    }
}
