using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Internal.Unknown;
using ToSic.Lib.Boot;
using ToSic.Lib.DI;
using ToSic.Sys.Configuration;

// ReSharper disable once CheckNamespace
namespace ToSic.Lib;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartUpLibCore
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IServiceCollection AddLibCore(this IServiceCollection services)
    {
        // Boot Sequence Stuff
        services.TryAddTransient<BootCoordinator>();


        services.TryAddTransient<IGlobalConfiguration, GlobalConfiguration>();

        // Warnings for mock implementations
        services.TryAddTransient(typeof(WarnUseOfUnknown<>));

        return services
            .AddLibLogging()
            .AddLibDiServiceSwitchers()
            .AddLibDiBasics();
    }
}