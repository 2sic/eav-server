#if NETFRAMEWORK

using System.Web;

namespace ToSic.Eav.WebApi.Sys.Helpers.Response;

internal static class WebApiResponseHelper
{
    /// <summary>
    ///  Extend Time so Web Server doesn't time out
    /// </summary>
    public static void PreventServerTimeout600(this TDotNetController apiController)
        => HttpContext.Current.Server.ScriptTimeout = 600;
}

#endif
