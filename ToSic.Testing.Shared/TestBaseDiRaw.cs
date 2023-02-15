using System;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Lib.DI;
using ToSic.Lib.Helpers;
using ToSic.Lib.Services;

namespace ToSic.Testing.Shared
{
    /// <summary>
    /// Very base class for Tests which use Dependency Injection.
    /// This base class only provides the infrastructure, but does not register any services yet.
    /// </summary>
    public abstract class TestBaseDiRaw : ServiceBase, IServiceBuilder
    {
        #region Constructor

        /// <summary>
        /// The base constructor will trigger calls to SetupServices.
        /// This ensures that anything that uses this test base or a derived class will have
        /// DI prepared in the way tests stack up
        /// </summary>
        /// <param name="logName"></param>
        protected TestBaseDiRaw(string logName = null) : base("Tst." + (logName ?? "BaseDI"))
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            SetupServices(ServiceCollection);
        }

        /// <summary>
        /// Setup services which are expected to be used.
        /// Override in your classes and remember to call the base.SetupServices
        /// </summary>
        /// <param name="services"></param>
        protected virtual void SetupServices(IServiceCollection services)
        {
        }

        #endregion

        #region Dependency Injection / Service Provider

        /// <summary>
        /// The underlying service collection, which will be used to create the Service Provider
        /// </summary>
        protected IServiceCollection ServiceCollection { get; } = new ServiceCollection();

        /// <summary>
        /// The configured service provider.
        /// It's private because it should never be used directly
        /// The first time it's accessed it will build itself.
        /// </summary>
        private IServiceProvider ServiceProvider => _serviceProvider.Get(() => ServiceCollection.BuildServiceProvider());

        private readonly GetOnce<IServiceProvider> _serviceProvider = new GetOnce<IServiceProvider>();

        /// <summary>
        /// The main helper to get services
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Build<T>() => ServiceProvider.Build<T>(Log);

        #endregion


    }
}
