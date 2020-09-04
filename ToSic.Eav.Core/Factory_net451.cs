using System;
using Microsoft.Extensions.DependencyInjection;
#if NETFRAMEWORK
using System.Web;
#endif

namespace ToSic.Eav
{
    public partial class Factory
    {
#if NETFRAMEWORK
        /// <summary>
        /// Dictionary key for keeping the Injection Service Provider in the Http-Context
        /// </summary>
        private const string ServiceProviderKey = "eav-scoped-serviceprovider";
        private static IServiceProvider _sp;

        private static IServiceProvider GetServiceProvider()
        {
            // Because 2sxc runs inside DNN as a webforms project and not asp.net core mvc, we have
            // to make sure the service-provider object is disposed correctly. If we don't do this,
            // connections to the database are kept open, and this leads to errors like "SQL timeout:
            // "All pooled connections were in use". https://github.com/2sic/2sxc/issues/1200
            // Work-around for issue https://github.com/2sic/2sxc/issues/1200
            // Scope service-provider based on request
            var httpCtx = HttpContext.Current;
            if (httpCtx == null) return _sp.CreateScope().ServiceProvider;

            if (httpCtx.Items[ServiceProviderKey] == null)
            {
                httpCtx.Items[ServiceProviderKey] = _sp.CreateScope().ServiceProvider;

                // Make sure service provider is disposed after request finishes
                httpCtx.AddOnRequestCompleted(context =>
                {
                    ((IDisposable)context.Items[ServiceProviderKey])?.Dispose();
                });
            }

            return (IServiceProvider)httpCtx.Items[ServiceProviderKey];
        }
#endif
    }
}
