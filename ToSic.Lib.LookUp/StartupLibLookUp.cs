using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.LookUp;

// ReSharper disable once CheckNamespace
namespace ToSic.Lib.LookUp;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class StartupLibLookUp
{
    public static IServiceCollection AddLibLookUp(this IServiceCollection services)
    {
        services.TryAddTransient<ILookUpEngineResolver, LookUpEngineResolverUnknown>();

        return services;
    }

}