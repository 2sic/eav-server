using System;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.DI;
using ToSic.Lib.Logging;

namespace ToSic.Testing.Shared
{
    public abstract class TestBaseWithCustomDependencyInjection: HasLog
    {
        protected IServiceProvider ServiceProvider => _serviceProvider;
        private IServiceProvider _serviceProvider;

        public T Resolve<T>() => ServiceProvider.Build<T>();

        protected TestBaseWithCustomDependencyInjection(Func<IServiceCollection, IServiceCollection> configure): base("Tst.BaseDI")
        {
            var services = new ServiceCollection() as IServiceCollection;
            services = configure(services);
            _serviceProvider = services.BuildServiceProvider();
        }

    }
}
