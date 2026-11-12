using System.Net;

namespace ToSic.Eav.WebApi.Sys.Helpers.Http;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class HttpException
{

    public static HttpExceptionAbstraction BadRequest(string message) => new(HttpStatusCode.BadRequest, message);

    public static HttpExceptionAbstraction NotAllowedFileType(string filename, string? message = null) 
        => new(HttpStatusCode.UnsupportedMediaType, $"file {filename} has an unsupported file type. {message}");

    public static HttpExceptionAbstraction PermissionDenied(string? message = null) 
        => new(HttpStatusCode.Forbidden, $"Permission denied. {message}");

    public static HttpExceptionAbstraction MissingParam(string paramName) 
        => new(HttpStatusCode.BadRequest, $"Param {paramName} missing.");
}