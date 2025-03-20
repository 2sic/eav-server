using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using ToSic.Lib.Services;

namespace ToSic.Lib.DI;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class StartUp
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IServiceCollection AddLibDiBasics(this IServiceCollection services)
    {
        // Lazy objects in General
        services.TryAddTransient(typeof(Lazy<>), typeof(LazyImplementation<>));

        // Lazy Services
        services.TryAddTransient(typeof(LazySvc<>));

        // Service Generators
        services.TryAddTransient(typeof(Generator<>));

        // Empty MyServices
        services.TryAddTransient<MyServicesEmpty>();

        return services;
    }

    /// <summary>
    /// Add Service Switchers
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IServiceCollection AddLibDiServiceSwitchers(this IServiceCollection services)
    {
        services.TryAddTransient(typeof(ServiceSwitcher<>));
        services.TryAddScoped(typeof(ServiceSwitcherScoped<>)); // note: it's for scoped, and we must use another object name here
        services.TryAddTransient(typeof(ServiceSwitcherSingleton<>)); // note: it's for singletons, but the service is transient on purpose!
        return services;
    }
}