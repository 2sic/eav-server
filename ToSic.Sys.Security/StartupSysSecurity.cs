using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.Security.Permissions;
using ToSic.Sys.Users;
using ToSic.Sys.Users.Permissions;

namespace ToSic.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupSysSecurity
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IServiceCollection AddSysSecurity(this IServiceCollection services)
    {
        services.TryAddTransient<ICurrentContextUserPermissions, CurrentContextUserPermissionsBasic>();
        services.TryAddTransient<ICurrentContextUserPermissionsService, CurrentContextUserPermissionsService>();

        // Permissions helper
        services.TryAddTransient<PermissionCheckBase.MyServices>();

        return services;
    }

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IServiceCollection AddSysSecurityFallback(this IServiceCollection services)
    {
        services.TryAddTransient<IUser, UserUnknown>();
        return services;
    }

}