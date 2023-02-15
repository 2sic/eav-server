using System;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Testing.Shared
{
    /// <summary>
    /// Very base class for Tests which use Dependency Injection.
    /// This base class only provides the infrastructure, but does not register any services yet.
    /// </summary>
    public abstract class TestBaseForIoC : ServiceBase, IServiceBuilder
    {
        #region Constructor

        public TestConfiguration TestConfiguration { get; }

        /// <summary>
        /// The base constructor will trigger calls to SetupServices.
        /// This ensures that anything that uses this test base or a derived class will have
        /// DI prepared in the way tests stack up
        /// </summary>
        protected TestBaseForIoC(TestConfiguration testConfiguration = default) : base("Tst.IoC")
        {
            // Store configuration
            TestConfiguration = testConfiguration ?? new TestConfiguration();

            // Initialize IoC
            var services = new ServiceCollection();
            // ReSharper disable once VirtualMemberCallInConstructor
            SetupServices(services);
            ServiceProvider = services.BuildServiceProvider();

            // Start any configuration code
            // ReSharper disable once VirtualMemberCallInConstructor
            Configure();
        }

        /// <summary>
        /// Setup services which are expected to be used.
        /// Override in your classes and remember to call the base.SetupServices
        /// </summary>
        /// <param name="services"></param>
        protected virtual void SetupServices(IServiceCollection services)
        {
        }

        /// <summary>
        /// Configuration code which will run after all services have been setup
        /// </summary>
        protected virtual void Configure()
        {

        }

        #endregion

        #region Use Dependency Injection / Service Provider

        /// <summary>
        /// The configured service provider.
        /// It's private because it should never be used directly
        /// The first time it's accessed it will build itself.
        /// </summary>
        private IServiceProvider ServiceProvider { get; }


        /// <summary>
        /// The main helper to get services
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Build<T>() => ServiceProvider.Build<T>(Log);

        #endregion


    }
}
