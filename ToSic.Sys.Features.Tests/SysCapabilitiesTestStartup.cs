using Microsoft.Extensions.DependencyInjection;
using ToSic.Sys.Run.Startup;

namespace ToSic.Sys.Features;

public static class SysCapabilitiesTestStartup
{
    /// <summary>
    /// Default DI registration, called by XUnit
    /// </summary>
    /// <param name="services"></param>
    public static IServiceCollection AddSysCapabilitiesAndSysCore(this IServiceCollection services)
        => services
            .AddSysCapabilities()
            .AddSysCapabilitiesFallbacks()
            .AddSysCore();

}