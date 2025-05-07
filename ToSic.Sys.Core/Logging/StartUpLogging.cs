using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ToSic.Lib.Logging;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class StartUpLogging
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IServiceCollection AddLibLogging(this IServiceCollection services)
    {
        // History (very core service)
        services.TryAddTransient<ILogStore, LogStoreLive>();
        services.TryAddTransient<ILogStoreLive, LogStoreLive>();

        return services;
    }
}