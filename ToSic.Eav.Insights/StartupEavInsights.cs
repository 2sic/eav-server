using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Sys.Insights;
using ToSic.Eav.Sys.Insights.App;
using ToSic.Eav.Sys.Insights.Data;
using ToSic.Eav.Sys.Insights.Logs;
using ToSic.Eav.Sys.Insights.Sys;
using InsightsControllerReal = ToSic.Eav.Sys.Insights.InsightsControllerReal;

namespace ToSic.Eav;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupEavInsights
{
    public static IServiceCollection AddEavInsights(this IServiceCollection services)
    {
        // Insights, the most important core backend
        services.TryAddTransient<InsightsControllerReal>();
        services.TryAddTransient<InsightsDataSourceCache>();

        services.AddTransient<IInsightsProvider, InsightsIsAlive>();
        services.AddTransient<IInsightsProvider, InsightsTypes>();
        services.AddTransient<IInsightsProvider, InsightsGlobalTypes>();
        services.AddTransient<IInsightsProvider, InsightsMemoryCache>();

        services.AddTransient<IInsightsProvider, InsightsLogs>();
        services.AddTransient<IInsightsProvider, InsightsPauseLogs>();
        services.AddTransient<IInsightsProvider, InsightsLogsFlush>();
        services.AddTransient<IInsightsProvider, InsightsAppStats>();
        services.AddTransient<IInsightsProvider, InsightsPurgeApp>();
        services.AddTransient<IInsightsProvider, InsightsAppsCache>();
        services.AddTransient<IInsightsProvider, InsightsAppLoadLog>();

        services.AddTransient<IInsightsProvider, InsightsGlobalTypesLog>();
        services.AddTransient<IInsightsProvider, InsightsTypeMetadata>();
        services.AddTransient<IInsightsProvider, InsightsTypePermissions>();

        services.AddTransient<IInsightsProvider, InsightsLicenses>();
        services.AddTransient<IInsightsProvider, InsightsAttributes>();
        services.AddTransient<IInsightsProvider, InsightsAttributeMetadata>();
        services.AddTransient<IInsightsProvider, InsightsAttributePermissions>();

        services.AddTransient<IInsightsProvider, InsightsEntity>();
        services.AddTransient<IInsightsProvider, InsightsEntities>();
        services.AddTransient<IInsightsProvider, InsightsEntityPermissions>();
        services.AddTransient<IInsightsProvider, InsightsEntityMetadata>();

        services.AddTransient<IInsightsProvider, InsightsHelp>();

        return services;
    }
}