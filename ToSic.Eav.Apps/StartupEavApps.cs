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
using ToSic.Eav.Environment.Sys.ServerPaths;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Run.Startup;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
public static class StartupEavApps
{
    public static IServiceCollection AddEavApps(this IServiceCollection services)
    {
        // App Readers and Factory
        services.TryAddTransient<AppReader>();
        services.TryAddTransient<IAppReaderFactory, AppReaderFactory>(); // new v18

        // State Builder for cache etc.
        services.TryAddTransient<IAppStateBuilder, AppState.AppStateBuilder>();

        // Global Content Types - needed by the Persistence Layer
        services.TryAddTransient<IGlobalDataService, GlobalDataService>();

        // App-State and Cache
        services.AddEavAppsCache();

        // Data Stack
        services.TryAddTransient<AppDataStackService>();

        // App Finder Helper for searching for apps by Name
        services.TryAddTransient<AppFinder>();

        return services;
    }

    public static IServiceCollection AddEavAppsCache(this IServiceCollection services)
    {
        // The true cache state as stored locally in memory
        services.TryAddSingleton<IAppsCacheInMemory, AppsCacheInMemory>();
        services.TryAddKeyedSingleton<IAppsCacheInMemory, AppsCacheInMemory>(AppsCacheInMemory.DefaultNameId); // New v22.03, preparing to use Keyed instead of Switchable

        // Register the default memory cache, which should only be used if something wants to override it.
        services.TryAddTransient<AppsCacheManager>();

        // Register with the default name
        services.TryAddKeyedTransient<IAppsCacheManager, AppsCacheManager>(AppsCacheManager.DefaultNameId); // New v22, preparing to use Keyed instead of Switchable

        // This is still WIP, depends a bit on how we implement the farm.
        services.TryAddScoped<IAppsCacheSelector, AppsCacheSelector>();
        services.TryAddTransient<IAppStateCacheService, AppStateCacheService>();
        services.TryAddTransient<IAppCacheKeyService, AppCacheKeyServiceDefault>();

        // Units of Work
        services.AddTransient<IAppCachePurge, AppCachePurge>(); // Implementation for the Database layer to use
        services.AddTransient<IZoneCachePurge, ZoneCachePurge>(); // Implementation for the Database layer to use
        services.AddTransient<IAppCacheUpdate, AppCacheUpdate>(); // Implementation for the Database layer to use

        // Catalog
        services.TryAddTransient<IAppsCatalog, AppsCatalog>(); // new v18
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
        services.TryAddTransient<IServerPaths, ServerPathsUnknown>();

        return services;
    }

}