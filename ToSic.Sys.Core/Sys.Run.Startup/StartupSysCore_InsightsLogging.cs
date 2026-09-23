using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace ToSic.Sys.Run.Startup;

public static partial class StartupSysCore
{
    public static ILoggingBuilder AddSysCoreInsightsLogger(this ILoggingBuilder logging)
    {
        logging.Services.AddSysCoreInsightsLoggerServices();
        return logging;
    }

    private static void AddSysCoreInsightsLoggerServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IInsightsLogStore, InsightsLogStore>();
        // Register the provider once, then expose that same instance through both service types.
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, InsightsLoggerProvider>());
        services.TryAddSingleton<InsightsLoggerProvider>(serviceProvider => serviceProvider.GetServices<ILoggerProvider>().OfType<InsightsLoggerProvider>().Single());
        // Only the Insights provider needs Trace; do not lower filters for the other host providers.
        services.Configure<LoggerFilterOptions>(options => options.Rules.Add(new(typeof(InsightsLoggerProvider).FullName, null, LogLevel.Trace, null)));
    }
}
