﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ToSic.Eav
{
	/// <summary>
	/// The Eav Factory, used to construct a DataSource
	/// </summary>
	public class Factory
	{

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
                // 2017-05-31 2rm Quick work-around for issue https://github.com/2sic/2sxc/issues/1200
	            return ServiceCollection.BuildServiceProvider();
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