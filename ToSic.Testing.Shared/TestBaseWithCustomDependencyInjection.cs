using System;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Logging;

namespace ToSic.Testing.Shared
{
    public abstract class TestBaseWithCustomDependencyInjection: HasLog
    {
        protected IServiceProvider ServiceProvider { get; }

        protected TestBaseWithCustomDependencyInjection(Func<IServiceCollection, IServiceCollection> configure): base("Tst.BaseDI")
        {
            var services = new ServiceCollection() as IServiceCollection;
            services = configure(services);
            ServiceProvider = services.BuildServiceProvider();
        }
    }
}
