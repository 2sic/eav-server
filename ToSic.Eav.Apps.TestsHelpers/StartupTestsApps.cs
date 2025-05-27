using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Integration;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Startup;
using ToSic.Eav.DataSources;
using ToSic.Eav.Integration;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.StartUp;
using ToSic.Eav.Testing;
using ToSic.Lib;
using ToSic.Sys;

#pragma warning disable CA1822

// ReSharper disable once CheckNamespace
namespace ToSic.Eav;

/// <summary>
/// A Startup helper for tests which need Dependency-Injection setup for EAV Core, DB and Apps.
/// </summary>
/// <remarks>
/// Use by adding this kind of attribute to your test class:
/// `[Startup(typeof(StartupTestsApps))]`
/// </remarks>
public class StartupTestsApps
{
    /// <summary>
    /// Startup helper
    /// </summary>
    public virtual void ConfigureServices(IServiceCollection services) =>
        services
            .AddFixtureHelpers()
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
            .AddEavDataPersistence()
            .AddEavDataBuild()
            .AddEavCore()
            .AddEavCoreFallbackServices()
            // Library
            .AddLibCore()
            .AddSysCapabilities()
            .AddSysCapabilitiesFallback();
}