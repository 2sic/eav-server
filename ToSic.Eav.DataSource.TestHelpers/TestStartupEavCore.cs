using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.DataSources;
using ToSic.Eav.StartUp;
using ToSic.Lib;
#pragma warning disable CA1822

namespace ToSic.Eav;

/// <summary>
/// A Startup helper for tests which need Dependency-Injection setup for EAV Core.
/// </summary>
/// <remarks>
/// Use by adding this kind of attribute to your test class:
/// `[Startup(typeof(TestStartupEavCore))]`
/// </remarks>
public class TestStartupEavCoreAndDataSources
{
    /// <summary>
    /// Startup helper
    /// </summary>
    public void ConfigureServices(IServiceCollection services) =>
        services
            .AddDataSources()
            .AddEavCore()
            .AddEavCoreFallbackServices()
            .AddLibCore();
}