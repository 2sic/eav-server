using Microsoft.Extensions.DependencyInjection;
using ToSic.Sys.Run.Startup;

namespace ToSic.Sys;

public static class StartupHelpers
{
    public static void SetupSysSecurity(this IServiceCollection services)
    {
        services
            .AddSysFeatures()
            .AddSysSecurity()
            .AddSysCore()
            .AddSysCapabilitiesFallbacks()
            .AddSysSecurityFallbacks();
    }
}