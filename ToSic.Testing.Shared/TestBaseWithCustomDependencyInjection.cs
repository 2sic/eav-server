using System;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Testing.Shared
{
    public abstract class TestBaseWithCustomDependencyInjection: ServiceBase
    {
        protected IServiceProvider ServiceProvider => _serviceProvider;
        private IServiceProvider _serviceProvider;

        public T Resolve<T>() => ServiceProvider.Build<T>(Log);

        protected TestBaseWithCustomDependencyInjection(Func<IServiceCollection, IServiceCollection> configure): base("Tst.BaseDI")
        {
            var services = new ServiceCollection() as IServiceCollection;
            services = configure(services);
            _serviceProvider = services.BuildServiceProvider();
        }

    }
}
