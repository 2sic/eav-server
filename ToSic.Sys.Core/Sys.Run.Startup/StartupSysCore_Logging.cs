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

    public static IServiceCollection AddSysCoreLegacyLogging(this IServiceCollection services)
    {
        RemoveLoggingServices(services);
        RemoveSysCoreInsightsLoggerServices(services);
        services.RemoveAll<IInsightsLogSnapshotReader>();
        services.AddSingleton<ILogFactory>(LegacyLogFactory.Instance);
        services.AddTransient<ILogStore, LogStoreLive>();
        services.AddTransient<ILogStoreLive, LogStoreLive>();
        services.AddTransient<IInsightsLogSnapshotReader, LegacyInsightsLogSnapshotReader>();

        return services;
    }

    public static IServiceCollection AddSysCoreMelLogging(this IServiceCollection services)
    {
        RemoveLoggingServices(services);
        services.AddSingleton<ILogFactory>(serviceProvider => new MelLogFactory(serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>()));
        services.AddTransient<ILogStore, MelLogStore>();

        return services;
    }

    public static IServiceCollection AddSysCoreMelInsightsLogging(this IServiceCollection services)
    {
        // First select core MEL, then add exactly one local Insights provider and snapshot reader.
        services.AddSysCoreMelLogging();
        services.RemoveAll<IInsightsLogSnapshotReader>();
        services.AddTransient<IInsightsLogSnapshotReader, MelInsightsLogSnapshotReader>();
        services.AddSysCoreInsightsLoggerServices();

        return services;
    }

    private static void RemoveLoggingServices(IServiceCollection services)
    {
        services.RemoveAll<ILogFactory>();
        services.RemoveAll<ILogStore>();
        services.RemoveAll<ILogStoreLive>();
    }
}
