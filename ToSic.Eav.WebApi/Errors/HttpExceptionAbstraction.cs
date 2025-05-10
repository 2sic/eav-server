using System.Net;
#if NETFRAMEWORK
using System.Net.Http;
using BaseType = System.Web.Http.HttpResponseException;
#else
using BaseType = System.Exception;
#endif
namespace ToSic.Eav.WebApi.Errors;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class HttpExceptionAbstraction: BaseType
{
    public HttpExceptionAbstraction(HttpStatusCode statusCode, string message, string title = null)
#if NETFRAMEWORK
        : base(new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(message),
            ReasonPhrase = title ?? "Error in 2sxc Content API - not allowed"
        })
#else
        : base("Error " + statusCode + " " + message)
#endif

    {
        Status = (int)statusCode;
        Value = message;
    }

    public int Status { get; set; } = 500;
    public string Value { get; set; }   // additional message, because sometimes we need to pick it up elsewhere
}
