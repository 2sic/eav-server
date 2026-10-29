using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.Context;
using ToSic.Sys.Context.Sys;
using ToSic.Sys.Security.Permissions;
using ToSic.Sys.Users;

#pragma warning disable IDE0130
namespace ToSic.Sys.Run.Startup;
#pragma warning restore IDE0130

[InternalApi_DoNotUse_MayChangeWithoutNotice]
public static class StartupSysSecurity
{
    /// <summary>
    /// Add security and permission check services.
    /// </summary>
    public static IServiceCollection AddSysSecurity(this IServiceCollection services)
    {
        services.TryAddTransient<IContextService, ContextOfUserService>();

        // Permissions helper
        services.TryAddTransient<PermissionCheck>();

        return services;
    }

    /// <summary>
    /// Add fallback services for security and systems check; also an empty <see cref="IUser"/> implementation.
    /// </summary>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IServiceCollection AddSysSecurityFallbacks(this IServiceCollection services)
    {
        services.TryAddTransient<IUser, UserUnknown>();
        services.TryAddTransient<IEnvironmentPermission, EnvironmentPermissionUnknown>();

        return services;
    }

}