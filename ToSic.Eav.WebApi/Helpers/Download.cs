//using System;
//using System.Net;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using ToSic.Eav.Plumbing;

//namespace ToSic.Eav.WebApi.Helpers
//{
//    public static class Download
//    {
//        public static HttpResponseMessage BuildDownload(string content, string fileName)
//        {
//            var response = new HttpResponseMessage(HttpStatusCode.OK)
//            {
//                Content = new StringContent(content)
//            };
//            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
//            {
//                FileName = fileName
//            };
//            if (fileName.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase))
//                response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeHelper.Json);
//            else if (fileName.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase))
//                response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeHelper.Xml);

//            return response;
//        }
//    }
//}
