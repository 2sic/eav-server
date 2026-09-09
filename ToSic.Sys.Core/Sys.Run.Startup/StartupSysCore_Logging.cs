using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace ToSic.Sys.Run.Startup;

public static partial class StartupSysCore
{
    
    public static IServiceCollection AddSysCoreLogging(this IServiceCollection services)
    {
        // History (very core service)
        services.TryAddSingleton<InsightsLogStore>();
        services.TryAddSingleton<InsightsLoggerProvider>();
        services.TryAddSingleton<LogStoreLive>();
        services.TryAddSingleton<ILogStore>(sp => sp.GetRequiredService<LogStoreLive>());
        services.TryAddSingleton<ILogStoreLive>(sp => sp.GetRequiredService<LogStoreLive>());
        services.AddLogging(logging => logging
            .AddFilter(MicrosoftLoggerEventSink.StoreCategory, LogLevel.None)
            .AddFilter<InsightsLoggerProvider>(MicrosoftLoggerEventSink.Category, LogLevel.Trace)
            .AddFilter<InsightsLoggerProvider>(MicrosoftLoggerEventSink.StoreCategory, LogLevel.Trace));
        services.AddSingleton<ILoggerProvider>(sp => sp.GetRequiredService<InsightsLoggerProvider>());

        return services;
    }
}
