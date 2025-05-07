using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.DataSources;
using ToSic.Eav.StartUp;
using ToSic.Eav.TestData;
using ToSic.Eav.Testing;
using ToSic.Lib;
using ToSic.Lib.Internal.FeatSys;

namespace ToSic.Eav;

/// <summary>
/// A Startup helper for tests which need Dependency-Injection setup for EAV Core.
/// </summary>
/// <remarks>
/// Use by adding this kind of attribute to your test class:
/// `[Startup(typeof(StartupCoreDataSourcesAndTestData))]`
/// </remarks>
public class StartupCoreDataSourcesAndTestData
{
    /// <summary>
    /// Startup helper
    /// </summary>
    public virtual void ConfigureServices(IServiceCollection services) =>
        services
            .AddDataSourceTestHelpers()
            .AddTransient<DataTableTrivial>()
            .AddTransient<DataTablePerson>()
            .AddDataSources()
            .AddEavCore()
            .AddEavCoreFallbackServices()
            .AddLibCore()
            .AddLibFeatSys();
}