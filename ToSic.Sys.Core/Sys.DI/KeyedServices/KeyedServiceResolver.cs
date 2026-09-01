using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Sys.DI;

/// <summary>
/// Helper to resolve keyed services from the DI container, and to discover which keys are available for a specific service.
/// </summary>
/// <remarks>
/// Added in v22, should still be considered work in progress.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public static class KeyedServiceResolver
{
    /// <summary>
    /// Retrieve all keys for a given service type.
    /// For discovering which keys are available for a specific service.
    /// </summary>
    /// <remarks>
    /// It requires that keyed services were registered using the <see cref="KeyedServiceRegistrationExtensions.AddKeyedTransientWithMarker"/>.
    ///
    /// Added in v22
    /// </remarks>
    /// <typeparam name="TService"></typeparam>
    /// <param name="provider"></param>
    /// <returns></returns>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public static IEnumerable<string> GetAllKeysForService<TService>(this IServiceProvider provider)
    {
        // 1. Resolve all markers registered for this interface
        var markers = provider
            .GetServices<KeyMarker<TService>>();

        // 2. Get distinct keys (in case multiple implementations use the same key)
        return markers
            .Select(m => m.Key)
            .Distinct();
    }

    /// <summary>
    /// Retrieve all keyed services for a given service type.
    /// Note that for each key, multiple services could be returned.
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <param name="provider"></param>
    /// <returns></returns>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public static IEnumerable<KeyValuePair<string, TService>> GetAllKeyedServices<TService>(this IServiceProvider provider)
    {
        // 2. Get distinct keys (in case multiple implementations use the same key)
        var distinctKeys = provider.GetAllKeysForService<TService>();

        // 3. Resolve and flatten all services for each discovered key
        return distinctKeys
            .SelectMany(key => provider
                .GetKeyedServices<TService>(key)
                .Select(service => new KeyValuePair<string, TService>(key, service))
            )
            .ToArray();
    }
}