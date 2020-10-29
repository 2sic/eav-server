using System.Net;
using System.Net.Http;
#if NETFRAMEWORK
using BaseType = System.Web.Http.HttpResponseException;
#else
using BaseType = System.Exception;
#endif
namespace ToSic.Eav.WebApi.Errors
{
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
            : base("Error " + statusCode.ToString() + " " + message)
#endif

        {
        }
    }
}
