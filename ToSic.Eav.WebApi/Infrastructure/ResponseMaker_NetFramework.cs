#if NETFRAMEWORK
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.WebApi.Infrastructure;

/// <summary>
/// Net Framework implementation of the ResponseMaker
/// </summary>
internal class ResponseMaker: IResponseMaker
{
    public void Init(System.Web.Http.ApiController apiController) => _apiController = apiController;

    private System.Web.Http.ApiController _apiController;

    private System.Web.Http.ApiController ApiController
        => _apiController ?? throw new Exception(
            $"Accessing the {nameof(ApiController)} in the {nameof(ResponseMaker)} requires it to be Init first.");

    public virtual HttpResponseMessage InternalServerError(string message)
        => Error((int)HttpStatusCode.InternalServerError, message);

    public virtual HttpResponseMessage InternalServerError(Exception exception)
        => Error((int)HttpStatusCode.InternalServerError, exception);


    public HttpResponseMessage Error(int statusCode, string message) 
        => ApiController.Request.CreateErrorResponse((HttpStatusCode)statusCode, message);

    public HttpResponseMessage Error(int statusCode, Exception exception)
        => ApiController.Request.CreateErrorResponse((HttpStatusCode)statusCode, exception);

    public HttpResponseMessage Json(object json)
    {
        var responseMessage = ApiController.Request.CreateResponse(HttpStatusCode.OK);
        responseMessage.Content = new StringContent(JsonSerializer.Serialize(json, JsonOptions.SafeJsonForHtmlAttributes), Encoding.UTF8, MimeHelper.Json);
        return responseMessage;
    }

    public HttpResponseMessage Ok() 
        => ApiController.Request.CreateResponse(HttpStatusCode.OK);

    public HttpResponseMessage File(string fileContent, string fileName, string fileType)
    {
        var fileBytes = Encoding.UTF8.GetBytes(fileContent);
        return File(new MemoryStream(fileBytes), fileName, fileType);
    }


    public HttpResponseMessage File(Stream fileContent, string fileName, string fileType)
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

    public HttpResponseMessage File(string fileContent, string fileName)
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

#endif