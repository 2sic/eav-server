using System;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Documentation;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav
{
    /// <summary>
    /// The Eav DI Factory, used to construct various objects through [Dependency Injection](xref:NetCode.DependencyInjection.Index).
    ///
    /// If possible avoid using this, as it's a workaround for code which is outside of the normal Dependency Injection and therefor a bad pattern.
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
	public partial class Factory
	{
        private static IServiceCollection _serviceCollection = new ServiceCollection();

        [PrivateApi]
        public static void UseExistingServices(IServiceCollection newServiceCollection)
            => _serviceCollection = newServiceCollection;

        public delegate void ServiceConfigurator(IServiceCollection service);

        [PrivateApi]
	    public static void ActivateNetCoreDi(ServiceConfigurator configure)
	    {
	        var sc = _serviceCollection;
	        configure.Invoke(sc);
#if NETFRAMEWORK
            _sp = sc.BuildServiceProvider();
#endif
	    }

#if !NETFRAMEWORK
        private static IServiceProvider GetServiceProvider() => _serviceCollection.BuildServiceProvider();
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
        /// <remarks>
        /// Avoid using at all cost - only DNN and test-code may use this!
        /// </remarks>
        [PrivateApi]
        public static T StaticBuild<T>() => GetServiceProvider().Build<T>();

        /// <summary>
        /// This is a special internal resolver for static objects
        /// Should only be used with extreme caution, as downstream objects
        /// May need more scope-specific stuff, why may be missing
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <remarks>
        /// Avoid using at all cost - only in obsolete EAV code which will be removed in 2sxc 13
        /// </remarks>
        [PrivateApi]
        [Obsolete("Use this in all EAV code to ensure we know the APIs to remove in v13")]
        public static T ObsoleteBuild<T>() => GetServiceProvider().Build<T>();


        public static void Dummy(object sp)
        {
            _serviceCollection = (IServiceCollection)sp;
        }
    }
}