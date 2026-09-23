using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ToSic.Eav.Run.Startup;
using ToSic.Eav.WebApi.Sys.Admin;
using ToSic.Sys.Logging;
using static Xunit.Assert;

namespace ToSic.Eav;

public class StartupEavLoggingTests
{
    [Theory]
    [InlineData(null, typeof(LegacyLogFactory), typeof(LogStoreLive), typeof(LegacyInsightsLogSnapshotReader))]
    [InlineData(false, typeof(LegacyLogFactory), typeof(LogStoreLive), typeof(LegacyInsightsLogSnapshotReader))]
    [InlineData(true, typeof(MelLogFactory), typeof(MelLogStore), typeof(MelInsightsLogSnapshotReader))]
    public void AddEavAll_RegistersLoggingAndConstructsSystemInfo(bool? useMel, Type factoryType, Type storeType, Type readerType)
    {
        var services = new ServiceCollection();
        services.AddLogging();

        if (useMel.HasValue)
            services.AddEavAll(useMel.Value);
        else
            services.AddEavAll();
        services.AddEavAllFallbacks().AddEavWebApiTypedAfterEav();

        using var provider = services.BuildServiceProvider();
        IsType(factoryType, Single(provider.GetServices<ILogFactory>()));
        IsType(storeType, Single(provider.GetServices<ILogStore>()));
        IsType(readerType, Single(provider.GetServices<IInsightsLogSnapshotReader>()));
        if (useMel == true)
        {
            Empty(provider.GetServices<ILogStoreLive>());
            IsType<InsightsLogStore>(Single(provider.GetServices<IInsightsLogStore>()));
            Same(provider.GetRequiredService<InsightsLoggerProvider>(), Single(provider.GetServices<ILoggerProvider>()));
        }
        else
        {
            IsType<LogStoreLive>(Single(provider.GetServices<ILogStoreLive>()));
            Empty(provider.GetServices<IInsightsLogStore>());
            Empty(provider.GetServices<ILoggerProvider>());
            Null(provider.GetService<InsightsLoggerProvider>());
        }

        NotNull(ActivatorUtilities.CreateInstance<SystemInfo>(provider));
    }
}
