﻿using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Apps.Integration;
using ToSic.Eav.Integration;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.StartUp;
using ToSic.Lib;
using ToSic.Testing;

#pragma warning disable CA1822

namespace ToSic.Eav.DataSource.DbTests;

/// <summary>
/// A Startup helper for tests which need Dependency-Injection setup for EAV Core.
/// </summary>
/// <remarks>
/// Use by adding this kind of attribute to your test class:
/// `[Startup(typeof(TestStartupEavCore))]`
/// </remarks>
public class StartupTestFullWithDb
{
    /// <summary>
    /// Startup helper
    /// </summary>
    public virtual void ConfigureServices(IServiceCollection services) =>
        services
            .AddTransient<FullDbFixtureHelper>()
            .AddTransient<DoFixtureStartup<ScenarioBasic>>()
            .AddTransient<DataSourcesTstBuilder>()
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