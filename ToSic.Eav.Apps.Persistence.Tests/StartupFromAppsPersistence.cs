using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Apps;
using ToSic.Eav.Data.Build;


namespace ToSic.Eav;

/// <summary>
/// A Startup helper for tests which need Dependency-Injection setup for EAV Core.
///
/// IMPORTANT: This file is also included as an imports in the TestFixtures project.
/// It's done that way, so that project can be decoupled from this project because sometimes we need to run tests in production builds.
/// </summary>
public static class StartupFromAppsPersistence
{
    public static IServiceCollection StartupTestsAppsPersistenceAndBelow(this IServiceCollection services)
    {
        // Register Main Services
        services
            .AddEavAppsPersistence()
            .AddEavContext()
            .AddEavApps()
            .AddEavPersistence()
            .AddEavDataBuild()
            .AddEavDataStack()
            .AddEavData()
            .AddEavCoreLibAndSys();

        // Register Fallback Services
        services
            .AddAppPersistenceFallbackServices()
            .AddContextFallbackServices()
            .AddAppFallbackServices()
            .AddEavDataBuildFallbacks()
            .AddEavDataFallbacks()
            .AddAllLibAndSysFallbacks();

        return services;
    }

}