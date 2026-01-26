using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.Boot;
using ToSic.Sys.Configuration;
using ToSic.Sys.Wrappers;

namespace ToSic.Sys.Run.Startup;

/// <summary>
/// Startup registration for all of ToSic.Sys.Core.
/// This is the innermost foundation of services.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public static partial class StartupSysCore
{
    /// <summary>
    /// Add all SysCore services
    /// </summary>
    /// <remarks>
    /// Call this method during application startup to ensure wrapper dependencies are available for injection.
    /// </remarks>
    /// <param name="services">The service collection to which the wrapper services will be added. Cannot be null.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance, enabling method chaining.</returns>
    public static IServiceCollection AddSysCore(this IServiceCollection services)
    {
        return services
            .AddSysCoreBoot()
            .AddSysCoreWrappers()
            .AddSysCoreLogging()
            .AddSysCoreDiServiceSwitchers()
            .AddSysCoreDi();
    }

    /// <summary>
    /// Add core boot and configuration services
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSysCoreBoot(this IServiceCollection services)
    {
        // Boot Sequence Stuff
        services.TryAddTransient<BootCoordinator>();

        // Global Configuration
        services.TryAddTransient<IGlobalConfiguration, GlobalConfiguration>();

        return services;
    }

    /// <summary>
    /// Registers the default system core wrapper services with the dependency injection container.
    /// </summary>
    /// <inheritdoc cref="AddSysCore"/>
    public static IServiceCollection AddSysCoreWrappers(this IServiceCollection services)
    {
        // Wrapper Factory - new it v21
        services.TryAddTransient<IWrapperFactory, WrapperFactory>();

        return services;
    }

}