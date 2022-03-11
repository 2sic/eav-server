﻿#if NETFRAMEWORK
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.WebApi.Plumbing
{
    public class ResponseMakerNetFramework: ResponseMaker<HttpResponseMessage>
    {
        public void Init(System.Web.Http.ApiController apiController) => _apiController = apiController;

        private System.Web.Http.ApiController _apiController;

        private System.Web.Http.ApiController ApiController
            => _apiController ?? throw new Exception(
                $"Accessing the {nameof(ApiController)} in the {nameof(ResponseMakerNetFramework)} requires it to be Init first.");

        public override HttpResponseMessage Error(int statusCode, string message) 
            => ApiController.Request.CreateErrorResponse((HttpStatusCode)statusCode, message);

        public override HttpResponseMessage Error(int statusCode, Exception exception)
            => ApiController.Request.CreateErrorResponse((HttpStatusCode)statusCode, exception);

        public override HttpResponseMessage Json(object json)
        {
            var responseMessage = ApiController.Request.CreateResponse(HttpStatusCode.OK);
            responseMessage.Content = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, MimeHelper.Json);
            return responseMessage;
        }

        public override HttpResponseMessage Ok() 
            => ApiController.Request.CreateResponse(HttpStatusCode.OK);

        public override HttpResponseMessage File(Stream fileContent, string fileName, string fileType)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(fileContent) };
            response.Content.Headers.ContentLength = fileContent.Length;
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(fileType);
            return response;
        }

        public override HttpResponseMessage File(string fileContent, string fileName)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(fileContent) };
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };
            if (fileName.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase))
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeHelper.Json);
            else if (fileName.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase))
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeHelper.Xml);

            return response;
        }
    }
}
#endif