using Microsoft.Extensions.DependencyInjection;
using System.Collections;

namespace ToSic.Sys.DI;

/// <summary>
/// Generic interface to get all keyed services by name.
/// </summary>
/// <typeparam name="TKey">Key type, usually string</typeparam>
/// <typeparam name="TService">Implementation type</typeparam>
public interface IAllKeyedServices<TKey, TService>
    : IEnumerable<KeyedService<TKey, TService>>
    where TService : class;

/// <summary>
/// Internal implementation
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TService"></typeparam>
/// <param name="provider"></param>
/// <param name="registry"></param>
internal sealed class AllKeyedServices<TKey, TService>(ISysServiceProvider provider, KeyedServiceRegistry registry)
    : IAllKeyedServices<TKey, TService>
    where TService : class
{
    public IEnumerator<KeyedService<TKey, TService>> GetEnumerator()
        => registry
            // get all the keys in the registry for this service type
            .GetKeys<TKey, TService>()
            // retrieve the services of these keys
            .SelectMany(
                key => provider.GetKeyedServices<TService>(key),
                (key, service) => new KeyedService<TKey, TService>(key, service))
            .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
