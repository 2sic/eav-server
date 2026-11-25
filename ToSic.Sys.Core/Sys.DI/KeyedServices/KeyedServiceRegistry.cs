using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Sys.DI;

internal sealed class KeyedServiceRegistry(IServiceCollection services)
{
    // Execution is deferred until first runtime access (after BuildServiceProvider completes).
    // Once evaluated, Lazy<T> automatically releases its reference to 'services'.
    private Lazy<ILookup<Type, object>> KeysByServiceType => field
        ??= new(() => services
            .Where(d => d.IsKeyedService
                        && d.ServiceKey != null
                        && d.ServiceKey != KeyedService.AnyKey)
            .ToLookup(d => d.ServiceType, d => d.ServiceKey!));


    public IEnumerable<TKey> GetKeys<TKey, TService>()
        => KeysByServiceType.Value[typeof(TService)]
            .OfType<TKey>()
            .Distinct();
}
