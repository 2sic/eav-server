using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ToSic.Sys.Run.Startup;

public static partial class StartupSysCore
{
    /// <summary>
    /// Add core Dependency Injection prats, such as Lazy, Generator, etc.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSysCoreDi(this IServiceCollection services)
    {
        // Lazy objects in General
        services.TryAddTransient(typeof(Lazy<>), typeof(LazyImplementation<>));

        // Lazy Services
        services.TryAddTransient(typeof(LazySvc<>));

        // Service Generators
        services.TryAddTransient(typeof(Generator<>));
        services.TryAddTransient(typeof(Generator<,>));

        // Warnings for mock implementations
        services.TryAddTransient(typeof(WarnUseOfUnknown<>));

        // Empty MyServices
        services.TryAddTransient<DependenciesEmpty>();

        return services;
    }

    /// <summary>
    /// Add Service Switchers
    /// </summary>
    public static IServiceCollection AddSysCoreDiServiceSwitchers(this IServiceCollection services)
    {
        services.TryAddTransient(typeof(ServiceSwitcher<>));
        services.TryAddScoped(typeof(ServiceSwitcherScoped<>)); // note: it's for scoped, and we must use another object name here
        services.TryAddTransient(typeof(ServiceSwitcherSingleton<>)); // note: it's for singletons, but the service is transient on purpose!
        return services;
    }
}