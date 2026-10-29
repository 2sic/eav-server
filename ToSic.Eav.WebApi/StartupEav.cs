using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.DataFormats.EavLight;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Run.Startup;

/// <summary>
/// Combined Eav Dependency Injection Startup
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public static class StartupEav
{
    /// <summary>
    /// Use this to set up the new DI container
    /// </summary>
    /// <param name="services"></param>
    /// <param name="useMel">Selects MEL logging with Insights instead of the default Legacy stack.</param>
    public static IServiceCollection AddEavAll(this IServiceCollection services, bool useMel = false)
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
            .AddEavModels()
            .AddEavData()
            .AddAllLibAndSys(useMel);

        return services;
    }

    /// <summary>
    /// Fallbacks for services which were not implemented - must come last
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddEavAllFallbacks(this IServiceCollection services)
    {
        services
            .AddContextFallbacks()
            .AddAppPersistenceFallbackServices()
            .AddAppFallbackServices()
            .AddEavImportExportFallbacks()
            .AddEavDataBuildFallbacks()
            .AddEavDataFallbacks()
            .AddAllLibAndSysFallbacks();
        return services;
    }
}
