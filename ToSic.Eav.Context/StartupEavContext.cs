using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps.Integration;
using ToSic.Eav.Cms.Internal.Languages;
using ToSic.Eav.Context;
using ToSic.Eav.Context.Internal;
using ToSic.Eav.Integration;
using ToSic.Eav.Integration.Security;
using ToSic.Eav.Internal.Environment;
using ToSic.Eav.Security.Internal;
using ToSic.Sys.Security.Permissions;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Apps;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupEavContext
{
    public static IServiceCollection AddEavContext(this IServiceCollection services)
    {
        //services.TryAddTransient<AppReader>();
        //services.TryAddTransient<IAppStateBuilder, AppState.AppStateBuilder>();

        //// Global Content Types - needed by the Persistence Layer
        //services.TryAddTransient<IGlobalContentTypesService, GlobalContentTypesService>();

        // Context
        services.TryAddTransient<IContextOfSite, ContextOfSite>();
        services.TryAddTransient<ContextOfSite>();
        services.TryAddTransient<ContextOfSite.MyServices>();

        //// Runtimes and Managers
        //services.TryAddTransient<AppCachePurger>();
        //services.TryAddTransient<AppFinder>();

        //// Context
        ////services.TryAddTransient<IContextOfApp, ContextOfApp>();
        ////services.TryAddTransient<ContextOfApp.MyServices>();

        services.TryAddTransient<IAppPathsMicroSvc, AppPathsMicroSvc>(); // WIP trying to remove direct access to AppPaths

        //// App-State and Cache
        //services.AddTransient<AppsCacheSwitch>();
        //services.AddTransient<IAppCachePurger, AppsCacheSwitch>(); // Implementation for the Database layer to use
        //services.TryAddTransient<IAppStateCacheService, AppStateCacheService>();
        //services.TryAddTransient<IAppsCatalog, AppsCatalog>(); // new v18
        //services.TryAddSingleton<IAppsCache, AppsCache>();
        //services.AddSingleton<IAppsCacheSwitchable, AppsCache>();
        //services.TryAddTransient<IAppReaderFactory, AppReaderFactory>(); // new v18

        //services.TryAddTransient<AppDataStackService>();

        // App Permission Check moved to this project as the implementations are now all identical
        services.TryAddTransient<AppPermissionCheck>();
        services.TryAddTransient<MultiPermissionsTypes>();
        services.TryAddTransient<MultiPermissionsApp>();
        services.TryAddTransient<MultiPermissionsApp.MyServices>();

        // V13 Language Checks
        services.TryAddTransient<AppUserLanguageCheck>();

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
    public static IServiceCollection AddContextFallbackServices(this IServiceCollection services)
    {
        services.TryAddTransient<ISite, SiteUnknown>();
        services.TryAddTransient<IZoneMapper, ZoneMapperUnknown>();
        services.TryAddTransient<AppPermissionCheck, AppPermissionCheckUnknown>();
        services.TryAddTransient<IEnvironmentPermission, EnvironmentPermissionUnknown>();

        services.TryAddTransient<IServerPaths, ServerPathsUnknown>();

        // Special registration of iisUnknown to verify we see warnings if such a thing is loaded
        //services.TryAddTransient<IIsUnknown, ServerPathsUnknown>();

        // v17
        services.TryAddTransient<IZoneCultureResolver, ZoneCultureResolverUnknown>();

        return services;
    }

}