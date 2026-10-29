#if NETFRAMEWORK
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using ToSic.Eav.Serialization.Sys.Json;

namespace ToSic.Eav.WebApi.Sys.Helpers.Http;

/// <summary>
/// Net Framework implementation of the ResponseMaker
/// </summary>
public class ResponseMakerNetFramework: IResponseMaker
{
    public void Init(TDotNetController apiController) => _apiController = apiController;

    private TDotNetController? _apiController;

    private TDotNetController ApiController
        => _apiController ?? throw new(
            $"Accessing the {nameof(ApiController)} in the {nameof(ResponseMakerNetFramework)} requires it to be Init first.");

    public virtual THttpResponseType InternalServerError(string message)
        => Error((int)HttpStatusCode.InternalServerError, message);

    public virtual THttpResponseType InternalServerError(Exception exception)
        => Error((int)HttpStatusCode.InternalServerError, exception);


    public THttpResponseType Error(int statusCode, string message) 
        => ApiController.Request.CreateErrorResponse((HttpStatusCode)statusCode, message);

    public THttpResponseType Error(int statusCode, Exception exception)
        => ApiController.Request.CreateErrorResponse((HttpStatusCode)statusCode, exception);

    public THttpResponseType Json(object json)
    {
        var responseMessage = ApiController.Request.CreateResponse(HttpStatusCode.OK);
        responseMessage.Content = new StringContent(JsonSerializer.Serialize(json, JsonOptions.SafeJsonForHtmlAttributes), Encoding.UTF8, MimeTypeConstants.Json);
        return responseMessage;
    }

    public THttpResponseType Ok() 
        => ApiController.Request.CreateResponse(HttpStatusCode.OK);

    public THttpResponseType File(string fileContent, string fileName, string fileType)
    {
        var fileBytes = Encoding.UTF8.GetBytes(fileContent);
        return File(new MemoryStream(fileBytes), fileName, fileType);
    }


    public THttpResponseType File(Stream fileContent, string fileName, string fileType)
    {
        var response = new THttpResponseType(HttpStatusCode.OK) { Content = new StreamContent(fileContent) };
        response.Content.Headers.ContentLength = fileContent.Length;
        response.Content.Headers.ContentDisposition = new("attachment")
        {
            FileName = fileName
        };
        response.Content.Headers.ContentType = new(fileType);
        return response;
    }

    public THttpResponseType File(string fileContent, string fileName)
    {
        var response = new THttpResponseType(HttpStatusCode.OK) { Content = new StringContent(fileContent) };
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