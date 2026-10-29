#if NETFRAMEWORK
using System.Web;
using ToSic.Eav.WebApi.Sys.Helpers.Http;

namespace ToSic.Eav.WebApi.Sys.Helpers.Response;

internal static class WebApiResponseHelper
{
    /// <summary>
    /// Helper to initialize the response maker.
    /// Demonstrates using the DI with a temporary injection override (scoped)
    /// </summary>
    /// <param name="apiController"></param>
    /// <returns></returns>
    public static IDisposable InitializedResponseMaker(this TDotNetController apiController)
    {
        var responseMaker = new ResponseMakerNetFramework();
        responseMaker.Init(apiController);
        return OverrideService<IResponseMaker>.Use(responseMaker);
    }

    /// <summary>
    ///  Extend Time so Web Server doesn't time out
    /// </summary>
    public static void PreventServerTimeout600(this TDotNetController apiController)
        => HttpContext.Current.Server.ScriptTimeout = 600;
}

#endif