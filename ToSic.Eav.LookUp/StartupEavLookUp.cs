using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.LookUp.Sys.Engines;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Run.Startup;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
public static class StartupEavLookUp
{
    public static IServiceCollection AddLibLookUpFallbacks(this IServiceCollection services)
    {
        services.TryAddTransient<ILookUpEngineResolver, LookUpEngineResolverUnknown>();

        return services;
    }

}