using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ToSic.Sys.DI;

/// <summary>
/// Helper for better managing keyed services in dependency injection.
/// This allows for adding a marker to the service registration, which can be used later to retrieve the key associated with the service.
/// </summary>
/// <remarks>
/// Added in v22, should still be considered work in progress.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public static class KeyedServiceRegistrationExtensions
{
    private static IServiceCollection AddKey<TService>(this IServiceCollection services, string key)
        => services.AddSingleton(new KeyMarker<TService>(key));

    /// <summary>
    /// Add keyed transient like classic Dependency injection, but with a marker to allow for later retrieval of the key.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public static IServiceCollection AddKeyedTransientWithMarker<TService, TImplementation>(this IServiceCollection services, string key)
        where TService : class
        where TImplementation : class, TService
    {
        return services
            .AddKeyedTransient<TService, TImplementation>(key)
            .AddKey<TService>(key);
    }

    /// <summary>
    /// Add keyed singleton like classic Dependency injection, but with a marker to allow for later retrieval of the key.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public static IServiceCollection AddKeyedSingletonWithMarker<TService, TImplementation>(this IServiceCollection services, string key)
        where TService : class
        where TImplementation : class, TService
    {
        return services
            .AddKeyedSingleton<TService, TImplementation>(key)
            .AddKey<TService>(key);
    }

    /// <summary>
    /// Add keyed scoped like classic Dependency injection, but with a marker to allow for later retrieval of the key.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public static IServiceCollection AddKeyedScopedWithMarker<TService, TImplementation>(this IServiceCollection services, string key)
        where TService : class
        where TImplementation : class, TService
    {
        return services
            .AddKeyedScoped<TService, TImplementation>(key)
            .AddKey<TService>(key);
    }

    /// <summary>
    /// Try to add keyed transient like classic Dependency injection, but with a marker to allow for later retrieval of the key.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public static IServiceCollection TryAddKeyedTransientWithMarker<TService, TImplementation>(this IServiceCollection services, string key)
        where TService : class
        where TImplementation : class, TService
    {
        services.TryAddKeyedTransient<TService, TImplementation>(key);
        return services.AddKey<TService>(key);
    }
    
    /// <summary>
    /// Try to add keyed singleton like classic Dependency injection, but with a marker to allow for later retrieval of the key.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public static IServiceCollection TryAddKeyedSingletonWithMarker<TService, TImplementation>(this IServiceCollection services, string key)
        where TService : class
        where TImplementation : class, TService
    {
        services.TryAddKeyedSingleton<TService, TImplementation>(key);
        return services.AddKey<TService>(key);
    }
    
    /// <summary>
    /// Try to add keyed scoped like classic Dependency injection, but with a marker to allow for later retrieval of the key.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public static IServiceCollection TryAddKeyedScopedWithMarker<TService, TImplementation>(this IServiceCollection services, string key)
        where TService : class
        where TImplementation : class, TService
    {
        services.TryAddKeyedScoped<TService, TImplementation>(key);
        return services.AddKey<TService>(key);
    }
}
