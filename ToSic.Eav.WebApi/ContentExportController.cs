using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using ToSic.Eav.ImportExport.Refactoring;
using ToSic.Eav.ImportExport.Refactoring.Options;

namespace ToSic.Eav.WebApi
{
    public class ContentExportController : Eav3WebApiBase
    {
        public class ContentExportArgs
        {
            public int AppId;

            public string DefaultLanguage;

            public string Language;

            public RecordExport RecordExport;

            public ResourceReferenceExport ResourcesReferences;

            public LanguageReferenceExport LanguageReferences;

            public string ContentType;
        }


        [HttpGet]
        public HttpResponseMessage ExportContent(int appId, string language, string defaultLanguage, string contentType, RecordExport recordExport, ResourceReferenceExport resourcesReferences, LanguageReferenceExport languageReferences)
        {
            AppId = appId;

            var contentTypeId = GetContentTypeId(contentType);
            var contentTypeName = GetContentTypeName(contentType);
            var contextLanguages = GetContextLanguages();

            string fileContent;
            if (recordExport.IsBlank())
            {
                fileContent = new XmlExport().CreateBlankXml(CurrentContext.ZoneId, appId, contentTypeId, "");
            }
            else
            {
                fileContent = new XmlExport().CreateXml(CurrentContext.ZoneId, appId, contentTypeId, language ?? "", defaultLanguage, contextLanguages, languageReferences, resourcesReferences);
            }

            var fileName = string.Format("2sxc {0} {1} {2} {3}.xml", contentTypeName.Replace(" ", "-"), language, recordExport.IsBlank() ? "Template" : "Data", DateTime.Now.ToString("yyyyMMddHHmmss"));

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(GetStreamFromString(fileContent));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
            response.Content.Headers.ContentLength = fileContent.Length;
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = fileName;
            return response;
        }


        private Stream GetStreamFromString(string str)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private int GetContentTypeId(string staticName)
        {
            return CurrentContext.AttribSet.GetAttributeSetId(staticName, null);
        }

        private string GetContentTypeName(string staticName)
        {
            return CurrentContext.AttribSet.GetAttributeSet(staticName).Name;
        }

        private string[] GetContextLanguages()
        {
            return CurrentContext.Dimensions.GetLanguages().Select(language => language.ExternalKey).ToArray();
        }
    }
}
