﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.DataFormats.EavLight;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Startup;

/// <summary>
/// Global Eav Configuration
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupEav
{
    /// <summary>
    /// Use this to setup the new DI container
    /// </summary>
    /// <param name="services"></param>
    public static IServiceCollection AddEavEverything(this IServiceCollection services)
    {
        // standard IEntity conversion
        // not sure where to put it, interface is in Core but the implementation in Web, also used by DataSources for json errors
        services.TryAddTransient<IConvertToEavLight, ConvertToEavLight>();

        services
            // WebAPI & Work
            .AddEavInsights()
            .AddEavWebApi()
            .AddEavWork()
            // Apps
            .AddEavAppsPersistence()
            .AddEavContext()
            .AddEavApps()
            // SQL Server
            .AddRepositoryAndEfc()
            // Import/Export as well as File Based Json loading
            .AddEavImportExport()
            .AddEavPersistence()
            // DataSources
            .AddDataSources()
            .AddDataSourceSystem()
            // EAV Core
            .AddEavDataBuild()
            .AddEavDataStack()
            .AddEavData()
            .AddEavCoreLibAndSys();

        return services;
    }

    /// <summary>
    /// Fallbacks for services which were not implemented - must come last
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddEavEverythingFallbacks(this IServiceCollection services)
    {
        services
            .AddContextFallbackServices()
            .AddAppPersistenceFallbackServices()
            .AddAppFallbackServices()
            .AddEavImportExportFallback()
            .AddEavDataBuildFallbacks()
            .AddEavDataFallbacks()
            .AddAllLibAndSysFallbacks();
        return services;
    }
}