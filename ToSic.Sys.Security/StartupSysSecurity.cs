using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.Security.Permissions;
using ToSic.Sys.Users;
using ToSic.Sys.Users.Permissions;

// ReSharper disable once CheckNamespace
namespace ToSic.Sys.Run.Startup;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
public static class StartupSysSecurity
{
    public static IServiceCollection AddSysSecurity(this IServiceCollection services)
    {
        services.TryAddTransient<ICurrentContextUserPermissions, CurrentContextUserPermissionsBasic>();
        services.TryAddTransient<ICurrentContextUserPermissionsService, CurrentContextUserPermissionsService>();

        // Permissions helper
        services.TryAddTransient<PermissionCheck>();

        return services;
    }

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IServiceCollection AddSysSecurityFallbacks(this IServiceCollection services)
    {
        services.TryAddTransient<IUser, UserUnknown>();
        services.TryAddTransient<IEnvironmentPermission, EnvironmentPermissionUnknown>();

        return services;
    }

}