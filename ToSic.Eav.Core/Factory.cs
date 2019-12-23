using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using ToSic.Eav.Apps;
using ToSic.Eav.Caching;
using ToSic.Eav.Documentation;

namespace ToSic.Eav
{
	/// <summary>
	/// The Eav DI Factory, used to construct various objects through Dependency Injection.
	/// </summary>
	[PublicApi_Stable_ForUseInYourCode]
	public class Factory
	{
#pragma warning disable IDE0051 // Remove unused private members
        // ReSharper disable once UnusedMember.Local
        private const string ServiceProviderKey = "2sxc-scoped-serviceprovider";
#pragma warning restore IDE0051 // Remove unused private members
        private static readonly IServiceCollection ServiceCollection = new ServiceCollection();

        public delegate void ServiceConfigurator(IServiceCollection service);

	    public static void ActivateNetCoreDi(ServiceConfigurator configure)
	    {
	        var sc = ServiceCollection;
	        configure.Invoke(sc);
	        _sp = sc.BuildServiceProvider();
	    }

        private static IServiceProvider ServiceProvider
	    {
	        get
	        {
                // Because 2sxc runs inside DNN as a webforms project and not asp.net core mvc, we have
                // to make sure the service-provider object is disposed correctly. If we don't do this,
                // connections to the database are kept open, and this leads to errors like "SQL timeout:
                // "All pooled connections were in use". https://github.com/2sic/2sxc/issues/1200

#if NETFULL
                // Scope service-provider based on request
                var httpContext = HttpContext.Current;
                if (httpContext == null) return _sp.CreateScope().ServiceProvider;

	            if (httpContext.Items[ServiceProviderKey] == null)
	            {
	                httpContext.Items[ServiceProviderKey] = _sp.CreateScope().ServiceProvider;

                    // Make sure service provider is disposed after request finishes
	                httpContext.AddOnRequestCompleted(context =>
	                {
	                    ((IDisposable) context.Items[ServiceProviderKey])?.Dispose();
	                });
                }

                return (IServiceProvider)httpContext.Items[ServiceProviderKey];

#else
                // 2017-05-31 2rm Quick work-around for issue https://github.com/2sic/2sxc/issues/1200
                return ServiceCollection.BuildServiceProvider();

#endif
            }
        }


#pragma warning disable IDE0052 // Remove unread private members
        // ReSharper disable once NotAccessedField.Local
        private static IServiceProvider _sp;
#pragma warning restore IDE0052 // Remove unread private members

        /// <summary>
        /// Internal debugging, disabled by default.
        /// </summary>
        public static bool Debug = false;

        /// <summary>
        /// Dependency Injection resolver with a known type as a parameter.
        /// </summary>
        /// <typeparam name="T">The type / interface we need.</typeparam>
        public static T Resolve<T>()
        {
            if (Debug) LogResolve(typeof(T), true);

            var found = ServiceProvider.GetService<T>();

            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (found == null) // unregistered type
                found = ActivatorUtilities.CreateInstance<T>(ServiceProvider);
            return found;
        }

        /// <summary>
        /// Dependency Injection resolver with a known type as a parameter.
        /// </summary>
        /// <param name="T">The type or interface we need</param>
        /// <returns></returns>
        public static object Resolve(Type T)
        {
            if (Debug) LogResolve(T, false);

            var found = ServiceProvider.GetService(T);

            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (found == null) // unregistered type
                found = ActivatorUtilities.CreateInstance(ServiceProvider, T);

            return found;
	    }

        /// <summary>
        /// Counter for internal statistics and debugging.
        /// </summary>
	    public static int CountResolves;

        public static List<string> ResolvesList = new List<string>();

	    public static void LogResolve(Type t, bool generic)
	    {
            CountResolves++;

            // Get call stack
            var stackTrace = new StackTrace();

            // Get calling method name
	        var mName = stackTrace.GetFrame(2).GetMethod().Name;
            
            ResolvesList.Add((generic ? "<>" : "()") + t.Name + "..." + mName);

	    }

        [PrivateApi]
        public static IAppsCache GetAppsCache()
        {
            if (_appsCacheSingleton != null) return _appsCacheSingleton;

            var appsCache = Resolve<IAppsCache>();
            if (appsCache.EnforceSingleton)
                _appsCacheSingleton = appsCache;
            return appsCache;
        }

        private static IAppsCache _appsCacheSingleton;
        [PrivateApi]
        public static AppState GetAppState(int appId) => GetAppsCache().Get(appId);

        [PrivateApi]
        public static AppState GetAppState(IAppIdentity app) => GetAppsCache().Get(app);

        [PrivateApi]
        public static IAppIdentity GetAppIdentity(int? zoneId, int? appId) => GetAppsCache().GetIdentity(zoneId, appId);
    }
}