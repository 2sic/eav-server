using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace ToSic.Lib.DI
{
    public static class StartUp
    {
        public static IServiceCollection AddLibDI(this IServiceCollection services)
        {
            // Lazy objects in General
            services.TryAddTransient(typeof(Lazy<>), typeof(LazyDependencyInjection<>));

            // Lazy Services
            services.TryAddTransient(typeof(LazySvc<>));

            // Service Generators
            services.TryAddTransient(typeof(Generator<>));

            // Service Switchers
            services.TryAddTransient(typeof(ServiceSwitcher<>));
            services.TryAddScoped(typeof(ServiceSwitcherScoped<>)); // note: it's for scoped, and we must use another object name here
            services.TryAddTransient(typeof(ServiceSwitcherSingleton<>)); // note: it's for singletons, but the service is transient on purpose!

            return services;
        }
    }
}
