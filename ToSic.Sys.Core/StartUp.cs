using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Internal.Configuration;
using ToSic.Lib.DI;

namespace ToSic.Lib;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class StartUp
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IServiceCollection AddLibCore(this IServiceCollection services)
    {
        services.TryAddTransient<IGlobalConfiguration, GlobalConfiguration>();

        return services
            .AddLibLogging()
            .AddLibDiServiceSwitchers()
            .AddLibDiBasics();
    }
}