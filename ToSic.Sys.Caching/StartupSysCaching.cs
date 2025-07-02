using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.Caching;

// ReSharper disable once CheckNamespace
namespace ToSic.Sys.Startup;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupSysCaching
{
    public static IServiceCollection AddSysCaching(this IServiceCollection services)
    {
        // v17
        services.TryAddTransient<MemoryCacheService>();

        return services;
    }

}