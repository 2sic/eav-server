using Microsoft.Extensions.DependencyInjection;
using System.Collections;

namespace ToSic.Sys.DI;

/// <summary>
/// Generic interface to get all keyed services by name.
/// </summary>
/// <remarks>
/// The services will only get instantiated if the enumerable is enumerated,
/// so this is a lazy way to get all services.
///
/// To only get the keys, use the <see cref="Keys"/> property, which will not instantiate any services.
/// </remarks>
/// <typeparam name="TKey">Key type, usually string</typeparam>
/// <typeparam name="TService">Implementation type</typeparam>
[InternalApi_DoNotUse_MayChangeWithoutNotice("WIP v22.02")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAllKeyedServices<TKey, TService>
    : IEnumerable<KeyValuePair<TKey, TService>>
    where TService : class
{
    /// <summary>
    /// Get a list of keys, without instantiating any services.
    /// </summary>
    IEnumerable<TKey> Keys { get; }
}

/// <summary>
/// Internal implementation
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TService"></typeparam>
internal sealed class AllKeyedServices<TKey, TService>(ISysServiceProvider provider, KeyedServiceRegistry registry)
    : IAllKeyedServices<TKey, TService>
    where TService : class
{
    public IEnumerable<TKey> Keys => registry.GetKeys<TKey, TService>();

    public IEnumerator<KeyValuePair<TKey, TService>> GetEnumerator()
        => registry
            // get all the keys in the registry for this service type
            .GetKeys<TKey, TService>()
            // retrieve the services of these keys
            .SelectMany(
                key => provider.GetKeyedServices<TService>(key),
                (key, service) => new KeyValuePair<TKey, TService>(key, service))
            .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
