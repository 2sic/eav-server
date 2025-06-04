#if NETFRAMEWORK
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using ToSic.Eav.Internal;
using ToSic.Eav.Serialization;
using ToSic.Eav.Serialization.Sys.Json;

namespace ToSic.Eav.WebApi.Infrastructure;

/// <summary>
/// Net Framework implementation of the ResponseMaker
/// </summary>
internal class ResponseMaker: IResponseMaker
{
    public void Init(System.Web.Http.ApiController apiController) => _apiController = apiController;

    private System.Web.Http.ApiController _apiController;

    private System.Web.Http.ApiController ApiController
        => _apiController ?? throw new(
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
        responseMessage.Content = new StringContent(JsonSerializer.Serialize(json, JsonOptions.SafeJsonForHtmlAttributes), Encoding.UTF8, MimeTypeConstants.Json);
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
        response.Content.Headers.ContentDisposition = new("attachment")
        {
            FileName = fileName
        };
        response.Content.Headers.ContentType = new(fileType);
        return response;
    }

    public HttpResponseMessage File(string fileContent, string fileName)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(fileContent) };
        response.Content.Headers.ContentDisposition = new("attachment")
        {
            FileName = fileName
        };
        if (fileName.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase))
            response.Content.Headers.ContentType = new(MimeTypeConstants.Json);
        else if (fileName.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase))
            response.Content.Headers.ContentType = new(MimeTypeConstants.Xml);

        return response;
    }
}

#endif