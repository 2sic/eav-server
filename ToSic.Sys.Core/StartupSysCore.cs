using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.Boot;
using ToSic.Sys.Configuration;

// ReSharper disable once CheckNamespace
namespace ToSic.Sys.Startup;

/// <summary>
/// Startup registration for all of ToSic.Sys.Core
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupSysCore
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IServiceCollection AddSysCore(this IServiceCollection services)
    {
        // Boot Sequence Stuff
        services.TryAddTransient<BootCoordinator>();

        // Global Configuration
        services.TryAddTransient<IGlobalConfiguration, GlobalConfiguration>();

        // Warnings for mock implementations
        services.TryAddTransient(typeof(WarnUseOfUnknown<>));

        // Empty MyServices
        services.TryAddTransient<MyServicesEmpty>();

        return services
            .AddSysLogging()
            .AddSysDiServiceSwitchers()
            .AddSysDi();
    }
}