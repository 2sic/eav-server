using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.LookUp.Sys.Engines;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.LookUp;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupLibLookUp
{
    public static IServiceCollection AddLibLookUpFallback(this IServiceCollection services)
    {
        services.TryAddTransient<ILookUpEngineResolver, LookUpEngineResolverUnknown>();

        return services;
    }

}