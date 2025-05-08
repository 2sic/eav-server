using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Context;
using ToSic.Eav.Internal.Configuration;
using ToSic.Lib.DI;

namespace ToSic.Lib;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class StartupSysSecurity
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IServiceCollection AddSysSecurity(this IServiceCollection services)
    {
        services.TryAddTransient<IUser, UserUnknown>();
        services.TryAddTransient<IContextOfUserPermissions, ContextOfUserPermissions>();
        services.TryAddTransient<IContextResolverUserPermissions, ContextResolverUserPermissions>();
        return services;
    }
}