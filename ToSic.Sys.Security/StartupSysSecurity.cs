using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.Users;
using ToSic.Sys.Users.Permissions;

namespace ToSic.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupSysSecurity
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IServiceCollection AddSysSecurity(this IServiceCollection services)
    {
        services.TryAddTransient<IUser, UserUnknown>();
        services.TryAddTransient<IContextOfUserPermissions, ContextOfUserPermissions>();
        services.TryAddTransient<IContextResolverUserPermissions, ContextResolverUserPermissions>();
        return services;
    }
}