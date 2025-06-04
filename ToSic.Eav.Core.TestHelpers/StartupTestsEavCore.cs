using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Data.Build;
using ToSic.Lib;
using ToSic.Sys;

#pragma warning disable CA1822

namespace ToSic.Eav;

/// <summary>
/// A Startup helper for tests which need Dependency-Injection setup for EAV Core.
/// </summary>
/// <remarks>
/// Use by adding this kind of attribute to your test class:
/// `[Startup(typeof(TestStartupEavCore))]`
/// </remarks>
public class StartupTestsEavCore
{
    /// <summary>
    /// Startup helper
    /// </summary>
    public virtual void ConfigureServices(IServiceCollection services) =>
        services
            //.AddEavDataPersistence()
            .AddEavDataBuild()
            .AddEavDataStack()
            .AddEavData()
            .AddEavCoreLibAndSys()

            .AddEavDataBuildFallbacks()
            .AddEavDataFallbacks()
            .AddAllLibAndSysFallbacks();
}