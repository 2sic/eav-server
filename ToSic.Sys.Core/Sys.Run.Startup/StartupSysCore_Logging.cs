using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ToSic.Sys.Run.Startup;

public static partial class StartupSysCore
{
    
    public static IServiceCollection AddSysCoreLogging(this IServiceCollection services)
    {
        services.TryAddSingleton<ILogFactory>(LegacyLogFactory.Instance);

        // History (very core service)
        services.TryAddTransient<ILogStore, LogStoreLive>();
        services.TryAddTransient<ILogStoreLive, LogStoreLive>();

        return services;
    }
}
