using System;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Documentation;

namespace ToSic.Eav
{
	/// <summary>
	/// The Eav DI Factory, used to construct various objects through Dependency Injection.
	/// </summary>
	[PublicApi_Stable_ForUseInYourCode]
	public partial class Factory
	{
        private static IServiceCollection _serviceCollection = new ServiceCollection();

        public static void BetaUseExistingServiceCollection(IServiceCollection newServiceCollection)
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
        private static IServiceProvider GetServiceProvider() => _serviceCollection.BuildServiceProvider();
#endif


        /// <summary>
        /// Dependency Injection resolver with a known type as a parameter.
        /// </summary>
        /// <typeparam name="T">The type / interface we need.</typeparam>
        public static T Resolve<T>()
        {
            if (Debug) LogResolve(typeof(T), true);

            var spToUse = GetServiceProvider();
            var found = spToUse.GetService<T>();
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (found != null) return found;

            // If it's an unregistered type, try to find in DLLs etc.
            return ActivatorUtilities.CreateInstance<T>(spToUse);
        }

        /// <summary>
        /// Dependency Injection resolver with a known type as a parameter.
        /// </summary>
        /// <param name="T">The type or interface we need</param>
        /// <returns></returns>
        public static object Resolve(Type T)
        {
            if (Debug) LogResolve(T, false);

            var spToUse = GetServiceProvider();
            var found = spToUse.GetService(T);
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (found != null) return found;

            // If it's an unregistered type, try to find in DLLs etc.
            return ActivatorUtilities.CreateInstance(spToUse, T);
        }
    }
}