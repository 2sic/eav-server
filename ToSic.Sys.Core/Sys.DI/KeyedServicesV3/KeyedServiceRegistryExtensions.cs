using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ToSic.Sys.DI;

public static class KeyedServiceRegistryExtensions
{
    ///// <summary>
    ///// Register the system to pick up all keys of all registered, named services.
    ///// </summary>
    ///// <remarks>
    ///// Call once anywhere in your startup configuration—order does not matter
    ///// </remarks>
    ///// <param name="services"></param>
    ///// <returns></returns>
    //public static IServiceCollection AddKeyedServiceRegistry(this IServiceCollection services)
    //{
    //    // Add the registry itself
    //    services.TryAddSingleton(new KeyedServiceRegistry(services));

    //    // Add the resolver/lookup
    //    services.TryAddTransient(typeof(IAllKeyedServices<,>), typeof(AllKeyedServices<,>));
    //    return services;
    //}

    // 2dm: for now this thing below is commented out, since we usually want to use constructor injection.

    ///// <summary>
    ///// Runtime resolution via IServiceProvider
    ///// </summary>
    ///// <typeparam name="TKey"></typeparam>
    ///// <typeparam name="TService"></typeparam>
    ///// <param name="provider"></param>
    ///// <returns></returns>
    //public static IEnumerable<KeyedService<TKey, TService>> GetKeyedServicesWithKeys<TKey, TService>(
    //    this ISysServiceProvider provider)
    //    where TService : class
    //{
    //    return [.. provider.GetRequiredService<IAllKeyedServices<TKey, TService>>()];
    //}
}
