using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.DataSources;
using ToSic.Eav.StartUp;
using ToSic.Eav.TestData;
using ToSic.Eav.Testing;
using ToSic.Lib;
using ToSic.Testing.Shared;

#pragma warning disable CA1822

namespace ToSic.Eav.StartupTests;

/// <summary>
/// A Startup helper for tests which need Dependency-Injection setup for EAV Core.
/// </summary>
/// <remarks>
/// Use by adding this kind of attribute to your test class:
/// `[Startup(typeof(TestStartupEavCore))]`
/// </remarks>
public class StartupTestsEavCoreAndDataSources
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
            .AddLibCore();
}