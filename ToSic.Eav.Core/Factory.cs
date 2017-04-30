using System;
using System.Collections.Generic;
using System.Configuration;
//using Microsoft.Practices.Unity;
//using Microsoft.Practices.Unity.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace ToSic.Eav
{
	/// <summary>
	/// The Eav Factory, used to construct a DataSource
	/// </summary>
	public class Factory
	{
		//private static IUnityContainer _container;

	 //   public static IUnityContainer CreateContainer()
	 //   {
	 //       _container = new UnityContainer();
	 //       return _container;
	 //   }

	    private static readonly IServiceCollection ServiceCollection = new ServiceCollection();

        public delegate void ServiceConfigurator(IServiceCollection service);

	    public static void ActivateNetCoreDi(ServiceConfigurator sc)
	    {
            UseCore = true;
	        sc(ServiceCollection);
	        _sp = ServiceCollection.BuildServiceProvider();
	    }

        private static IServiceProvider ServiceProvider
	    {
	        get
	        {
	            if (_sp != null) return _sp;
	            throw new Exception("service provider not built yet");
	        }
	    }

	    private static IServiceProvider _sp;

  //      /// <summary>
  //      /// The IoC Container responsible for our Inversion of Control
  //      /// Use this everywhere!
  //      /// Syntax: Factory.Container.Resolve<Type>
  //      /// </summary>
  //      public static IUnityContainer Container
		//{
		//	get
		//	{
		//	    if (_container != null) return _container;

		//	    CreateContainer();
		//	    //_container = new UnityContainer();

		//	    var section = (UnityConfigurationSection)ConfigurationManager.GetSection("unity");
		//	    if (section?.Containers["ToSic.Eav"] != null)
		//	        _container.LoadConfiguration("ToSic.Eav");
		//	    return _container;
		//	}
		//}

	    public static bool UseCore = false;
	    public static bool Debug = false;
        public static t Resolve<t>()
        {
            if (Debug) LogResolve(typeof(t), true);

            // if(!UseCore) return Container.Resolve<t>();

            var found = ServiceProvider.GetService<t>();

            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (found == null) // unregistered type
                found = ActivatorUtilities.CreateInstance<t>(ServiceProvider);
            return found;
        }

        public static object Resolve(Type t)
        {
            if (Debug) LogResolve(t, false);

            //if(!UseCore) return Container.Resolve(t);

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