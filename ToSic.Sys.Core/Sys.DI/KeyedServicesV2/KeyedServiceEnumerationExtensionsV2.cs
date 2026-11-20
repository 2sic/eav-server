using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Sys.DI;

public static class KeyedServiceEnumerationExtensionsV2
{
    // Call this on IServiceCollection before BuildServiceProvider()
    public static IServiceCollection AddKeyedServiceEnumeration<TKey, TService>(
        this IServiceCollection services)
        where TService : class
    {
        // Discover all distinct keys registered for TService
        var keys = services
            .Where(d => d.ServiceType == typeof(TService) && d is { IsKeyedService: true, ServiceKey: TKey })
            .Select(d => (TKey)d.ServiceKey)
            .Distinct()
            .ToList();

        // Register a transient resolver that pairs each key with its resolved service(s)
        services.AddTransient<IEnumerable<KeyedService<TKey, TService>>>(sp =>
        [
            .. keys.SelectMany(key =>
                sp.GetKeyedServices<TService>(key)
                    .Select(instance => new KeyedService<TKey, TService>(key, instance)))
        ]);

        return services;
    }

    // Convenience helper for runtime resolution via IServiceProvider
    public static IEnumerable<KeyedService<TKey, TService>> GetKeyedServicesWithKeysV2<TKey, TService>(
        this IServiceProvider provider)
        where TService : class
    {
        return provider.GetRequiredService<IEnumerable<KeyedService<TKey, TService>>>();
    }
}