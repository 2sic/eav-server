using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.DataSources;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.StartUp;
using ToSic.Eav.WebApi;
using ToSic.Lib;
using ToSic.Sys;

namespace ToSic.Eav.Integration;

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
            // EAV Core
            .AddEavDataBuild()


            .AddEavCoreLibAndSys()


            // Fallbacks for services which were not implemented - must come last
            .AddAppFallbackServices()
            .AddEavImportExportFallback()
            .AddEavDataBuildFallbacks()
            .AddEavCoreLibAndSysFallbackServices();

        return services;
    }
}