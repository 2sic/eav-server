using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Internal.Configuration;
using ToSic.Lib.DI;

namespace ToSic.Lib;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartUpLibCore
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IServiceCollection AddLibCore(this IServiceCollection services)
    {
        services.TryAddTransient<IGlobalConfiguration, GlobalConfiguration>();

        return services
            .AddLibLogging()
            .AddLibDiServiceSwitchers()
            .AddLibDiBasics();
    }
}