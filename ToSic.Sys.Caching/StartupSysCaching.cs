using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.Caching;

// ReSharper disable once CheckNamespace
namespace ToSic.Sys.Startup;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupSysCaching
{
    /// <summary>
    /// Add System Caching to dependency injection.
    /// </summary>
    public static IServiceCollection AddSysCaching(this IServiceCollection services)
    {
        // v17
        services.TryAddTransient<MemoryCacheService>();

        // v20
        services.TryAddScoped(typeof(ScopedCache<>));

        return services;
    }

}