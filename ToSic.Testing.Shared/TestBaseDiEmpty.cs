using System;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;

namespace ToSic.Testing.Shared
{
    public abstract class TestBaseDiEmpty : HasLog
    {
        private IServiceProvider ServiceProvider { get; }

        public T Build<T>() => ServiceProvider.Build<T>();

        protected TestBaseDiEmpty() : this(null) {}

        protected TestBaseDiEmpty(string logName = null) : base("Tst." + (logName ?? "BaseDI"))
        {
            ServiceProvider = SetupServices().BuildServiceProvider();
        }

        protected virtual IServiceCollection SetupServices(IServiceCollection services = null)
        {
            services = services ?? new ServiceCollection();
            AddServices(services);
            return services;
        }

        protected virtual void AddServices(IServiceCollection services) { }
    }
}
