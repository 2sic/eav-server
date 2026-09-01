using Microsoft.Extensions.DependencyInjection;
using ToSic.Sys.Run.Startup;

namespace ToSic.Sys.Features;

public static class StartupHelpers
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

    /// <summary>
    /// By default, it only has 2 feature checkers
    /// - Features
    /// - SysFeatures
    /// </summary>
    public const int RequirementChecksInDiByDefault = 2;
}