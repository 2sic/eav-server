namespace ToSic.Eav.WebApi.Sys.Helpers.Http;

public interface IHttpExceptionMaker
{
    Exception BadRequest(string message);
    Exception PermissionDenied(string message = null);
}