using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Startup;
using ToSic.Eav.DataSources;
using ToSic.Eav.Integration;
using ToSic.Eav.StartUp;
using ToSic.Eav.TestData;
using ToSic.Eav.Testing;
using ToSic.Lib;
using ToSic.Sys;

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
            .AddEavPersistence()
            .AddEavDataBuild()
            .AddEavCore()
            .AddEavCoreFallbackServices()
            .AddLibCore()
            .AddSysCapabilities()
            .AddSysCapabilitiesFallback();
}