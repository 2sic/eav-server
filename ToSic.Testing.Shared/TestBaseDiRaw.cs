using System;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Testing.Shared
{
    public abstract class TestBaseDiRaw : ServiceBase, IServiceBuilder
    {
        private IServiceProvider ServiceProvider { get; }

        public T Build<T>() => ServiceProvider.Build<T>();

        protected TestBaseDiRaw(string logName = null) : base("Tst." + (logName ?? "BaseDI"))
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            ServiceProvider = SetupServices(new ServiceCollection()).BuildServiceProvider();
        }

        protected virtual IServiceCollection SetupServices(IServiceCollection services)
        {
            AddServices(services);
            return services;
        }

        protected virtual void AddServices(IServiceCollection services) { }

    }
}
