using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps.Sys.Paths;
using ToSic.Eav.Apps.Sys.Permissions;
using ToSic.Eav.Context;
using ToSic.Eav.Context.Sys;
using ToSic.Eav.Context.Sys.Site;
using ToSic.Eav.Context.Sys.ZoneCulture;
using ToSic.Eav.Context.Sys.ZoneMapper;
using ToSic.Eav.Environment.Sys.Permissions;
using ToSic.Eav.Environment.Sys.ServerPaths;
using ToSic.Sys.Security.Permissions;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Apps;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupEavContext
{
    public static IServiceCollection AddEavContext(this IServiceCollection services)
    {
        // Context
        services.TryAddTransient<IContextOfSite, ContextOfSite>();
        services.TryAddTransient<ContextOfSite>();
        services.TryAddTransient<ContextOfSite.MyServices>();

        //// Context
        ////services.TryAddTransient<IContextOfApp, ContextOfApp>();
        ////services.TryAddTransient<ContextOfApp.MyServices>();

        services.TryAddTransient<IAppPathsMicroSvc, AppPathsMicroSvc>(); // WIP trying to remove direct access to AppPaths

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

        // v17
        services.TryAddTransient<IZoneCultureResolver, ZoneCultureResolverUnknown>();

        return services;
    }

}