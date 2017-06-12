using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Web;

namespace ToSic.Eav
{
	/// <summary>
	/// The Eav Factory, used to construct a DataSource
	/// </summary>
	public class Factory
	{
	    private const string ServiceProviderKey = "2sxc-scoped-serviceprovider";
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
                // to make sure the serviceprovider object is disposed correctly. If we don't do this,
                // connections to the database are kept open, and this leads to errors like "SQL timeout:
                // "All pooled connections were in use". https://github.com/2sic/2sxc/issues/1200
                
                // Scope serviceprovider based on request
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

	            // 2017-06-01 2dm attempt to use "child" scoped provider
	            //return _sp.CreateScope().ServiceProvider;

                // 2017-05-31 2rm Quick work-around for issue https://github.com/2sic/2sxc/issues/1200
                // return ServiceCollection.BuildServiceProvider();

                //if (_sp != null) return _sp;
                //throw new Exception("service provider not built yet");
            }
        }

	    private static IServiceProvider _sp;
        

	    public static bool Debug = false;
        public static t Resolve<t>()
        {
            if (Debug) LogResolve(typeof(t), true);

            var found = ServiceProvider.GetService<t>();

            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (found == null) // unregistered type
                found = ActivatorUtilities.CreateInstance<t>(ServiceProvider);
            return found;
        }

        public static object Resolve(Type t)
        {
            if (Debug) LogResolve(t, false);

            var found = ServiceProvider.GetService(t);

            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (found == null) // unregistered type
                found = ActivatorUtilities.CreateInstance(ServiceProvider, t);

            return found;
	    }

	    public static int CountResolves;
        public static List<string> ResolvesList = new List<string>();

	    public static void LogResolve(Type t, bool generic)
	    {
            CountResolves++;

            // Get call stack
            StackTrace stackTrace = new StackTrace();

            // Get calling method name
	        var mName = stackTrace.GetFrame(2).GetMethod().Name;
            
            ResolvesList.Add((generic ? "<>" : "()") + t.Name + "..." + mName);

	    }
	}
}