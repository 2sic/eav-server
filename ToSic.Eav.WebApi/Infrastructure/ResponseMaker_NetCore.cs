#if !NETFRAMEWORK
using System.IO;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using ToSic.Eav.Internal;

namespace ToSic.Eav.WebApi.Infrastructure;

/// <summary>
/// Net Core implementation of the Response Maker
/// </summary>
internal class ResponseMaker: IResponseMaker
{
    public void Init(ControllerBase apiController) => _apiController = apiController;

    private ControllerBase _apiController;

    public ControllerBase ApiController => _apiController ??
                                       throw new(
                                           $"Accessing the {nameof(ApiController)} in the {nameof(ResponseMaker)} requires it to be Init first.");

    public IActionResult InternalServerError(string message)
        => Error((int)HttpStatusCode.InternalServerError, message);

    public IActionResult InternalServerError(Exception exception)
        => Error((int)HttpStatusCode.InternalServerError, exception);


    public IActionResult Error(int statusCode, string message)
        => ApiController.Problem(message, null, statusCode); 

    public IActionResult Error(int statusCode, Exception exception)
        => ApiController.Problem(exception.Message, null, statusCode);

    public IActionResult Json(object json) => new JsonResult(json);

    public IActionResult Ok() => ApiController.Ok();

    public IActionResult File(string fileContent, string fileName, string fileType)
    {
        var fileBytes = Encoding.UTF8.GetBytes(fileContent);
        return File(new MemoryStream(fileBytes), fileName, fileType);
    }

    public IActionResult File(Stream fileContent, string fileName, string fileType)
    {
        using var memoryStream = new MemoryStream();
        fileContent.CopyTo(memoryStream);
        return new FileContentResult(memoryStream.ToArray(), fileType) { FileDownloadName = fileName };
    }

    public IActionResult File(string fileContent, string fileName)
    {
        new FileExtensionContentTypeProvider().TryGetContentType(fileName, out var contentType);
        return File(fileContent, fileName, contentType ?? MimeTypeConstants.FallbackType);
    }
}

#endif