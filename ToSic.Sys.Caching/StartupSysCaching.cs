using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Lib.Caching;

namespace ToSic.Eav.StartUp;

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