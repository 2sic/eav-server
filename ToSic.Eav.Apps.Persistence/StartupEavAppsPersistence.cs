using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps.Sys.AppJson;
using ToSic.Eav.Apps.Sys.AppStateInFolder;
using ToSic.Eav.Apps.Sys.FileSystemState;
using ToSic.Eav.Apps.Sys.Initializers;
using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.Apps.Sys.PresetLoaders;
using ToSic.Eav.Repositories.Sys;
using ToSic.Sys.Boot;
using ToSic.Sys.Utils.Assemblies;


// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Apps;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupEavAppsPersistence
{
    public static IServiceCollection AddEavAppsPersistence(this IServiceCollection services)
    {
        // App Loaders
        services.TryAddTransient<IAppLoaderTools, AppLoaderTools>();

        services.AddTransient<IBootProcess, BootWarmUpAssemblies>();
        services.AddTransient<IBootProcess, EavBootLoadPresetApp>();
        services.TryAddTransient<AppLoaderLogSettings>();   // new v20

        // core things - usually not replaced
        services.TryAddTransient<IAppStateLoader, AppStateInFolderLoader>();

        // File System Loaders
        services.TryAddTransient<IAppInputTypesLoader, AppFileSystemInputTypesLoader>();
        services.TryAddTransient<IAppContentTypesLoader, AppFileSystemContentTypesLoader>();

        // v17
        services.TryAddTransient<IAppJsonConfigurationService, AppJsonConfigurationService>();

        return services;
    }

    /// <summary>
    /// This will add Do-Nothing services which will take over if they are not provided by the main system
    /// In general this will result in some features missing, which many platforms don't need or care about
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    /// <remarks>
    /// All calls in here MUST use TryAddTransient, and never without the Try
    /// </remarks>
    public static IServiceCollection AddAppPersistenceFallbackServices(this IServiceCollection services)
    {
        services.TryAddTransient<IAppInputTypesLoader, AppInputTypesLoaderUnknown>();

        // Unknown-Runtime for loading configuration etc. File-runtime
        services.TryAddTransient<IAppContentTypesLoader, AppContentTypesLoaderUnknown>();

        services.TryAddTransient<IAppStateLoader, AppStateLoaderUnknown>();

        services.TryAddTransient<IStorageFactory, StorageFactoryUnknown>();

        services.TryAddTransient<IAppInitializedChecker, AppInitializedCheckerUnknown>();

        return services;
    }

}