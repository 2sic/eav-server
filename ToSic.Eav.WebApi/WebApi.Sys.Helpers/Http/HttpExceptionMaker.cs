namespace ToSic.Eav.WebApi.Sys.Helpers.Http;
internal class HttpExceptionMaker : IHttpExceptionMaker
{
    public Exception BadRequest(string message)
        => HttpException.BadRequest(message);

    public Exception PermissionDenied(string message = null)
        => HttpException.PermissionDenied(message);
}
