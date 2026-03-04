using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Run.Startup;

#pragma warning disable CA1822

namespace ToSic.Eav;

/// <summary>
/// A Startup helper for tests which need Dependency-Injection setup for EAV Core.
/// </summary>
/// <remarks>
/// Use by adding this kind of attribute to your test class:
/// `[Startup(typeof(TestStartupEavCore))]`
/// </remarks>
public class StartupTestsEavDataBuild
{
    /// <summary>
    /// Startup helper
    /// </summary>
    public virtual void ConfigureServices(IServiceCollection services) =>
        services
            //.AddDataSourceTestHelpers()

            .AddEavDataBuild()
            .AddEavModels()
            .AddEavData()
            .AddAllLibAndSys()

            .AddEavDataBuildFallbacks()
            .AddEavDataFallbacks()
            .AddAllLibAndSysFallbacks();
}