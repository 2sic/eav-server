namespace ToSic.Eav.WebApi.Sys.Helpers.Http;


[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IResponseMaker
{
    void Init(TDotNetController controller);

    THttpResponseType InternalServerError(string message);
    THttpResponseType InternalServerError(Exception exception);
    THttpResponseType Error(int statusCode, string message);
    THttpResponseType Error(int statusCode, Exception exception);
    THttpResponseType Json(object json);
    THttpResponseType Ok();
    THttpResponseType File(Stream fileContent, string fileName, string fileType);
    THttpResponseType File(string fileContent, string fileName, string fileType);
    THttpResponseType File(string fileContent, string fileName);
}
