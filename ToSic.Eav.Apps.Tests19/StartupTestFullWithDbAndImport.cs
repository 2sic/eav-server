using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Apps.Integration;
using ToSic.Eav.DataSources;
using ToSic.Eav.Integration;
using ToSic.Eav.StartUp;
using ToSic.Lib;

#pragma warning disable CA1822

namespace ToSic.Eav.Repository.Efc.Tests19;

// NOTE: QUITE A FEW DUPLICATES OF THIS - may want to consolidate

/// <summary>
/// A Startup helper for tests which need Dependency-Injection setup for EAV Core.
/// </summary>
/// <remarks>
/// Use by adding this kind of attribute to your test class:
/// `[Startup(typeof(TestStartupEavCore))]`
/// </remarks>
public class StartupTestFullWithDbAndImport
{
    /// <summary>
    /// Startup helper
    /// </summary>
    public virtual void ConfigureServices(IServiceCollection services) =>
        services
            //.AddTransient<FullDbFixtureHelper>()
            // not sure if this is needed or used here...probably not
            //.AddTransient<IImportExportEnvironment, ImportExportEnvironmentMock>()
            // Apps
            .AddEavApps()
            .AddAppFallbackServices()

            // SQL Server
            .AddRepositoryAndEfc()
            // Import/Export as well as File Based Json loading
            .AddImportExport()
            // DataSources
            .AddDataSources()
            // EAV Core
            .AddEavCore()
            .AddEavCoreFallbackServices()
            // Library
            .AddLibCore();
}