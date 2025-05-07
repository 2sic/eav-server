using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps.Integration;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.DataSources;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.StartUp;
using ToSic.Eav.WebApi;
using ToSic.Lib;
using ToSic.Lib.Internal.FeatSys;

namespace ToSic.Eav.Integration;

/// <summary>
/// Global Eav Configuration
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
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
            // WebAPI
            .AddEavWebApi()
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
            .AddLibCore()
            .AddLibFeatSys();

        return services;
    }
}