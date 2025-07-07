using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace ToSic.Sys.Startup;

/// <summary>
/// Startup for Dependency Injection.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
// ReSharper disable once InconsistentNaming
public static class StartupDI
{
    /// <summary>
    /// Add core Dependency Injection prats, such as Lazy, Generator, etc.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSysDi(this IServiceCollection services)
    {
        // Lazy objects in General
        services.TryAddTransient(typeof(Lazy<>), typeof(LazyImplementation<>));

        // Lazy Services
        services.TryAddTransient(typeof(LazySvc<>));

        // Service Generators
        services.TryAddTransient(typeof(Generator<>));
        services.TryAddTransient(typeof(Generator<,>));

        return services;
    }

    /// <summary>
    /// Add Service Switchers
    /// </summary>
    public static IServiceCollection AddSysDiServiceSwitchers(this IServiceCollection services)
    {
        services.TryAddTransient(typeof(ServiceSwitcher<>));
        services.TryAddScoped(typeof(ServiceSwitcherScoped<>)); // note: it's for scoped, and we must use another object name here
        services.TryAddTransient(typeof(ServiceSwitcherSingleton<>)); // note: it's for singletons, but the service is transient on purpose!
        return services;
    }
}