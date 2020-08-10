using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using ToSic.Eav.Documentation;

namespace ToSic.Eav
{
	/// <summary>
	/// The Eav DI Factory, used to construct various objects through Dependency Injection.
	/// </summary>
	[PublicApi_Stable_ForUseInYourCode]
	public class Factory
	{
#if NETFULL
        private const string ServiceProviderKey = "eav-scoped-serviceprovider";
        private static IServiceProvider _sp;
#endif
        private static IServiceCollection _serviceCollection = new ServiceCollection();

        public static void BetaUseExistingServiceCollection(IServiceCollection newServiceCollection)
            => _serviceCollection = newServiceCollection;

        public delegate void ServiceConfigurator(IServiceCollection service);

	    public static void ActivateNetCoreDi(ServiceConfigurator configure)
	    {
	        var sc = _serviceCollection;
	        configure.Invoke(sc);
#if NETFULL
	        _sp = sc.BuildServiceProvider();
#endif
	    }

        private static IServiceProvider ServiceProvider
	    {
	        get
	        {
#if NETFULL
                // Because 2sxc runs inside DNN as a webforms project and not asp.net core mvc, we have
                // to make sure the service-provider object is disposed correctly. If we don't do this,
                // connections to the database are kept open, and this leads to errors like "SQL timeout:
                // "All pooled connections were in use". https://github.com/2sic/2sxc/issues/1200
                // 2017-05-31 2rm Quick work-around for issue https://github.com/2sic/2sxc/issues/1200
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
                return _serviceCollection.BuildServiceProvider();
#endif
            }
        }


        /// <summary>
        /// Internal debugging, disabled by default. If set to true, resolves will be counted and logged.
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
        /// Counter for internal statistics and debugging. Will only be incremented if Debug = true.
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

    }
}