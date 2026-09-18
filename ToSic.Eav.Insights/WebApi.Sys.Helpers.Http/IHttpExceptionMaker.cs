namespace ToSic.Eav.WebApi.Sys.Helpers.Http;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IHttpExceptionMaker
{
    Exception BadRequest(string message);
    Exception PermissionDenied(string? message = null);
}