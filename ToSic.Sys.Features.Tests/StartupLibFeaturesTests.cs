using Microsoft.Extensions.DependencyInjection;
using ToSic.Sys;

namespace ToSic.Lib.Features.Tests;

/// <summary>
/// Class to inherit from, in test folders which should just do the auto-startup
/// </summary>
public class AutoStartupLibFeaturesTests
{
    /// <summary>
    /// Default DI registration, called by XUnit
    /// </summary>
    /// <param name="services"></param>
    public void ConfigureServices(IServiceCollection services) =>
        services
            .StartupLibFeaturesTests();
}

public static class StartupLibFeatures
{
    /// <summary>
    /// Default DI registration, called by XUnit
    /// </summary>
    /// <param name="services"></param>
    public static IServiceCollection StartupLibFeaturesTests(this IServiceCollection services)
        => services
            .AddSysCapabilities()
            .AddSysCapabilitiesFallback()
            .AddLibCore();
}