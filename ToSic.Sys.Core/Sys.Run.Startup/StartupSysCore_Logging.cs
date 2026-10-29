using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ToSic.Sys.Run.Startup;

public static partial class StartupSysCore
{
    /// <summary>
    /// Registers one complete logging stack. Call once at startup; changing stacks requires an application restart.
    /// Factory, store and reader use the same selection so Insights cannot resolve a mixed stack.
    /// </summary>
    public static IServiceCollection AddSysCoreLogging(this IServiceCollection services, bool useMel = false)
        => useMel
            ? services.AddSysCoreMelInsightsLogging()
            : services.AddSysCoreLegacyLogging();

    public static IServiceCollection AddSysCoreLegacyLogging(this IServiceCollection services)
    {
        services.TryAddSingleton<ILogFactory>(LegacyLogFactory.Instance);
        services.AddTransient<ILogStore, LogStoreLive>();
        services.AddTransient<ILogStoreLive, LogStoreLive>();
        services.AddTransient<IInsightsLogSnapshotReader, LegacyInsightsLogSnapshotReader>();

        return services;
    }

    public static IServiceCollection AddSysCoreMelLogging(this IServiceCollection services)
    {
        // Older DNN hosts have no ILoggerFactory; AddLogging supplies one without replacing a host factory.
        services.AddLogging();
        services.TryAddSingleton<ILogFactory>(serviceProvider => new MelLogFactory(serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>()));
        services.AddTransient<ILogStore, MelLogStore>();

        return services;
    }

    public static IServiceCollection AddSysCoreMelInsightsLogging(this IServiceCollection services)
    {
        // Complete the MEL stack with the local Insights provider and snapshot reader.
        services.AddSysCoreMelLogging();
        services.AddTransient<IInsightsLogSnapshotReader, MelInsightsLogSnapshotReader>();
        services.AddSysCoreInsightsLoggerServices();

        return services;
    }
}
