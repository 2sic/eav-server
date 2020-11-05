using System;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Documentation;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav
{
	/// <summary>
	/// The Eav DI Factory, used to construct various objects through Dependency Injection.
	/// </summary>
	[PublicApi_Stable_ForUseInYourCode]
	public partial class Factory
	{
        private static IServiceCollection _serviceCollection = new ServiceCollection();

        // ReSharper disable once UnusedMember.Global
        public static void UseExistingServices(IServiceCollection newServiceCollection)
            => _serviceCollection = newServiceCollection;

        public delegate void ServiceConfigurator(IServiceCollection service);

	    public static void ActivateNetCoreDi(ServiceConfigurator configure)
	    {
	        var sc = _serviceCollection;
	        configure.Invoke(sc);
#if NETFRAMEWORK
            _sp = sc.BuildServiceProvider();
#endif
	    }

#if !NETFRAMEWORK
        public static IServiceProvider GetServiceProvider() => _serviceCollection.BuildServiceProvider();
#endif


        /// <summary>
        /// Dependency Injection resolver with a known type as a parameter.
        /// </summary>
        /// <typeparam name="T">The type / interface we need.</typeparam>
        public static T Resolve<T>()
        {
            if (Debug) LogResolve(typeof(T), true);

            return GetServiceProvider().Build<T>();
        }

        /// <summary>
        /// This is a special internal resolver for static objects
        /// Should only be used with extreme caution, as downstream objects
        /// May need more scope-specific stuff, why may be missing
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T StaticBuild<T>() => GetServiceProvider().Build<T>();

        /// <summary>
        /// Dependency Injection resolver with a known type as a parameter.
        /// </summary>
        /// <param name="T">The type or interface we need</param>
        /// <returns></returns>
        public static object Resolve(Type T)
        {
            if (Debug) LogResolve(T, false);
            return GetServiceProvider().Build<object>(T);
        }
    }
}