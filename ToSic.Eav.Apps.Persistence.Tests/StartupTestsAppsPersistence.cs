using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Apps;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSources;
using ToSic.Eav.Integration;
using ToSic.Eav.StartUp;


namespace ToSic.Eav;

/// <summary>
/// A Startup helper for tests which need Dependency-Injection setup for EAV Core.
/// </summary>
/// <remarks>
/// Use by adding this kind of attribute to your test class:
/// `[Startup(typeof(StartupCoreDataSourcesAndTestData))]`
/// </remarks>
public class StartupTestsAppsPersistence
{
    /// <summary>
    /// Startup helper
    /// </summary>
    public virtual void ConfigureServices(IServiceCollection services)
        => StartupTestsAppsPersistenceAndBelow(services);

    public static IServiceCollection StartupTestsAppsPersistenceAndBelow(IServiceCollection services)
    {
        // Register Main Services
        services
            .AddEavAppsPersistence()
            .AddEavContext()
            .AddEavApps()
            .AddEavPersistence()
            .AddEavDataBuild()
            .AddEavCoreLibAndSys();

        // Register Fallback Services
        services
            .AddAppPersistenceFallbackServices()
            .AddContextFallbackServices()
            .AddAppFallbackServices()
            .AddEavDataBuildFallbacks()
            .AddEavDataFallbacks()
            .AddEavCoreLibAndSysFallbackServices();

        return services;
    }

}