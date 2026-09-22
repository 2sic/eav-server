using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace ToSic.Sys.Run.Startup;

public static partial class StartupSysCore
{
    public static ILoggingBuilder AddSysCoreInsightsLogger(this ILoggingBuilder logging)
    {
        logging.Services.TryAddSingleton<IInsightsLogStore, InsightsLogStore>();
        logging.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, InsightsLoggerProvider>());
        logging.Services.TryAddSingleton<InsightsLoggerProvider>(serviceProvider => serviceProvider.GetServices<ILoggerProvider>().OfType<InsightsLoggerProvider>().Single());
        logging.AddFilter<InsightsLoggerProvider>(null, LogLevel.Trace);
        return logging;
    }
}
