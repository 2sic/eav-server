using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Eav.DataSource.DbTests;

/// <summary>
/// A Startup helper for tests which need Dependency-Injection setup for EAV Core.
/// </summary>
/// <remarks>
/// Use by adding this kind of attribute to your test class:
/// `[Startup(typeof(TestStartupEavCore))]`
/// </remarks>
public class StartupTestFullWithDb: StartupTestsApps
{
    /// <summary>
    /// Startup helper
    /// </summary>
    public override void ConfigureServices(IServiceCollection services) =>
        base.ConfigureServices(
            services
                .AddDataSourceTestHelpers()
        );
}