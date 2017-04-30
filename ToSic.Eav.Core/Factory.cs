using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics;

namespace ToSic.Eav
{
	/// <summary>
	/// The Eav Factory, used to construct a DataSource
	/// </summary>
	public class Factory
	{
		private static IUnityContainer _container;

	    public static IUnityContainer CreateContainer()
	    {
	        _container = new UnityContainer();
	        return _container;
	    }

	    public static IServiceCollection ServiceCollection = new ServiceCollection();

        //public static IServiceCollection CreateServiceCollection()
        //{
        //    ServiceCollection = new ServiceCollection();
        //    return ServiceCollection;
        //}

        public delegate IServiceCollection ServiceConfigurator(IServiceCollection service);

	    public static void ActivateNetCoreDi(ServiceConfigurator sc)
	    {
            UseCore = true;
            _sp = sc(ServiceCollection).BuildServiceProvider();
	        // return _sp;
	    }

        private static IServiceProvider ServiceProvider
	    {
	        get
	        {
	            if (_sp != null) return _sp;
	            throw new Exception("service provider not built yet");
	        }
	        set { _sp = value; }
	    }

	    private static IServiceProvider _sp;

        /// <summary>
        /// The IoC Container responsible for our Inversion of Control
        /// Use this everywhere!
        /// Syntax: Factory.Container.Resolve<Type>
        /// </summary>
        public static IUnityContainer Container
		{
			get
			{
			    if (_container != null) return _container;

			    CreateContainer();
			    //_container = new UnityContainer();

			    var section = (UnityConfigurationSection)ConfigurationManager.GetSection("unity");
			    if (section?.Containers["ToSic.Eav"] != null)
			        _container.LoadConfiguration("ToSic.Eav");
			    return _container;
			}
		}

	    public static bool UseCore = false;
	    public static bool Debug = false;
        public static t Resolve<t>()
        {
            if (Debug)
                LogResolve(typeof(t), true);

            return UseCore
                ? ServiceProvider.GetService<t>()
                : Container.Resolve<t>();
        }

        public static object Resolve(Type t)
        {
            if (Debug)
                LogResolve(t, false);

            return UseCore
	            ? ServiceProvider.GetService(t)
	            : Container.Resolve(t);
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