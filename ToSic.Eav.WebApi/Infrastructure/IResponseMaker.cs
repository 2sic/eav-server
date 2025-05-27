#if NETFRAMEWORK
using THttpResponseType = System.Net.Http.HttpResponseMessage;
using TController = System.Web.Http.ApiController;
#else
using THttpResponseType = Microsoft.AspNetCore.Mvc.IActionResult;
using TController = Microsoft.AspNetCore.Mvc.ControllerBase;
#endif

namespace ToSic.Eav.WebApi.Infrastructure;


[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IResponseMaker
{
    void Init(TController controller);

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
