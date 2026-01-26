using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Apps.Sys.AppStack;
using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Apps.Sys.Catalog;
using ToSic.Eav.Apps.Sys.State;
using ToSic.Eav.Apps.Sys.State.AppStateBuilder;
using ToSic.Eav.Data.Global.Sys;
using ToSic.Eav.Data.Sys.Global;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Run.Startup;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
public static class StartupEavApps
{
    public static IServiceCollection AddEavApps(this IServiceCollection services)
    {
        services.TryAddTransient<AppReader>();
        services.TryAddTransient<IAppStateBuilder, AppState.AppStateBuilder>();

        // Global Content Types - needed by the Persistence Layer
        services.TryAddTransient<IGlobalDataService, GlobalDataService>();

        // Runtimes and Managers
        services.TryAddTransient<AppCachePurger>();
        services.TryAddTransient<AppFinder>();

        // App-State and Cache
        services.AddTransient<AppsCacheSwitch>();
        services.AddTransient<IAppCachePurger, AppsCacheSwitch>(); // Implementation for the Database layer to use
        services.TryAddTransient<IAppStateCacheService, AppStateCacheService>();
        services.TryAddTransient<IAppsCatalog, AppsCatalog>(); // new v18
        services.TryAddSingleton<IAppsCache, AppsCache>();
        services.AddSingleton<IAppsCacheSwitchable, AppsCache>();
        services.TryAddTransient<IAppReaderFactory, AppReaderFactory>(); // new v18

        services.TryAddTransient<AppDataStackService>();

        services.TryAddTransient<IRuntimeKeyService, RuntimeKeyServiceDefault>();

        return services;
    }

    /// <summary>
    /// This will add Do-Nothing services which will take over if they are not provided by the main system
    /// In general this will result in some features missing, which many platforms don't need or care about
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    /// <remarks>
    /// All calls in here MUST use TryAddTransient, and never without the Try
    /// </remarks>
    public static IServiceCollection AddAppFallbackServices(this IServiceCollection services)
    {
        return services;
    }

}