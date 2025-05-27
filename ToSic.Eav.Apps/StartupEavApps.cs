using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps.Catalog;
using ToSic.Eav.Apps.Integration;
using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.Internal.Api01;
using ToSic.Eav.Apps.Internal.MetadataDecorators;
using ToSic.Eav.Apps.Internal.Work;
using ToSic.Eav.Apps.Services;
using ToSic.Eav.Caching;
using ToSic.Eav.Cms.Internal.Languages;
using ToSic.Eav.Context;
using ToSic.Eav.Context.Internal;
using ToSic.Eav.ImportExport.Internal;
using ToSic.Eav.ImportExport.Internal.XmlList;
using ToSic.Eav.ImportExport.Internal.Zip;
using ToSic.Eav.Integration;
using ToSic.Eav.Integration.Environment;
using ToSic.Eav.Integration.Security;
using ToSic.Eav.Internal.Environment;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Internal.Requirements;
using ToSic.Eav.Security.Internal;
using ToSic.Sys.Security.Permissions;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Apps;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupEavApps
{
    public static IServiceCollection AddEavApps(this IServiceCollection services)
    {
        // Context
        services.TryAddTransient<IContextOfSite, ContextOfSite>();
        services.TryAddTransient<ContextOfSite>();

        // Runtimes and Managers
        services.TryAddTransient<ZoneManager>();
        services.TryAddTransient<AppCachePurger>();
        services.TryAddTransient<AppFinder>();

        // Runtime parts
        services.TryAddTransient<MdRecommendations>(); // new v13
        services.TryAddTransient<MdRequirements>(); // new v13
        services.TryAddTransient<IRequirementsService, MdRequirements>(); // new v16.08

        // New part v16 with better architecture
        services.TryAddTransient<AppWorkContextService>();
        services.TryAddTransient(typeof(GenWorkPlus<>));
        services.TryAddTransient(typeof(GenWorkDb<>));
        services.TryAddTransient(typeof(GenWorkBasic<>));
        services.TryAddTransient<WorkEntities>();
        services.TryAddTransient<WorkInputTypes>();
        services.TryAddTransient<WorkEntitySave>();
        services.TryAddTransient<WorkEntityCreate>();
        services.TryAddTransient<WorkEntityUpdate>();
        services.TryAddTransient<WorkMetadata>();
        services.TryAddTransient<WorkFieldList>();
        services.TryAddTransient<WorkEntityDelete>();
        services.TryAddTransient<WorkEntityPublish>();
        services.TryAddTransient<WorkEntityVersioning>();
        services.TryAddTransient<WorkContentTypesMod>();
        services.TryAddTransient<WorkAttributes>();
        services.TryAddTransient<WorkAttributesMod>();
        services.TryAddTransient<WorkQueryMod>();
        services.TryAddTransient<WorkQueryCopy>();

        // More services

        services.TryAddTransient<ImportService>();

        services.TryAddTransient<ZipExport>();
        services.TryAddTransient<ZipImport>();
        services.TryAddTransient<ZipImport.MyServices>();
        services.TryAddTransient<ZipFromUrlImport>();

        // App Dependencies
        services.TryAddTransient<EavApp.MyServices>();

        // Context
        services.TryAddTransient<IContextOfApp, ContextOfApp>();
        services.TryAddTransient<ContextOfApp.MyServices>();
        services.TryAddTransient<ContextOfSite.MyServices>();
        services.TryAddTransient<IAppPathsMicroSvc, AppPathsMicroSvc>(); // WIP trying to remove direct access to AppPaths

        // App Loaders
        services.TryAddTransient<IAppLoaderTools, AppLoaderTools>();
        services.TryAddTransient<SystemLoader>();
        services.TryAddTransient<EavSystemLoader>();
        services.TryAddTransient<EavFeaturesLoader>();  // new v20 separate class
        services.TryAddTransient<FeaturePersistenceService>();
        services.TryAddTransient<FeaturesIoHelper>();
        services.TryAddTransient<AppLoaderLogSettings>();   // new v20
        services.TryAddTransient<LicenseLoader>();

        // App-State and Cache
        services.AddTransient<AppsCacheSwitch>();   // Transient should work... wip
        services.TryAddTransient<IAppStateCacheService, AppStateCacheService>();
        services.TryAddTransient<IAppsCatalog, AppsCatalog>(); // new v18
        services.TryAddSingleton<IAppsCache, AppsCache>();
        services.AddSingleton<IAppsCacheSwitchable, AppsCache>();
        services.TryAddTransient<IAppReaderFactory, AppReaderFactory>(); // new v18

        services.TryAddTransient<AppDataStackService>();


        // File System Loaders
        services.TryAddTransient<IAppInputTypesLoader, AppFileSystemInputTypesLoader>();
        services.TryAddTransient<IAppContentTypesLoader, AppFileSystemContentTypesLoader>();

        // Helpers to build stuff
        services.TryAddTransient<AppCreator>();
        services.TryAddTransient<ZoneCreator>();
        services.TryAddTransient<IAppInitializedChecker, AppInitializedChecker>();
        services.TryAddTransient<AppInitializedChecker>();
        services.TryAddTransient<AppInitializer>();
        
        

        // export import stuff
        services.TryAddScoped<ExportListXml>();
        services.TryAddScoped<ImportListXml>();
        services.TryAddTransient<ExportImportValueConversion>();
        services.TryAddTransient<XmlImportWithFiles>(); // Note: added v19.03.03 2025-03-11 by 2dm https://github.com/2sic/2sxc/issues/3598
        services.TryAddTransient<XmlImportWithFiles.MyServices>();

        // Simple DataController - registration was missing
        services.TryAddTransient<SimpleDataEditService>();

        // App Permission Check moved to this project as the implementations are now all identical
        services.TryAddTransient<AppPermissionCheck>();
        services.TryAddTransient<MultiPermissionsTypes>();
        services.TryAddTransient<MultiPermissionsApp>();
        services.TryAddTransient<MultiPermissionsApp.MyServices>();

        // V13 Language Checks
        services.TryAddTransient<AppUserLanguageCheck>();

        // v17
        services.TryAddTransient<IAppJsonService, AppJsonService>();

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
    public static IServiceCollection AddAppFallbackServices(this IServiceCollection services)
    {
        services.TryAddTransient<IEnvironmentLogger, EnvironmentLoggerUnknown>();
        services.TryAddTransient<XmlExporter, XmlExporterUnknown>();
        services.TryAddTransient<ISite, SiteUnknown>();
        services.TryAddTransient<IZoneMapper, ZoneMapperUnknown>();
        services.TryAddTransient<AppPermissionCheck, AppPermissionCheckUnknown>();
        services.TryAddTransient<IEnvironmentPermission, EnvironmentPermissionUnknown>();
        services.TryAddTransient<IAppInputTypesLoader, AppInputTypesLoaderUnknown>();
        services.TryAddTransient<IImportExportEnvironment, ImportExportEnvironmentUnknown>();

        // v17
        services.TryAddTransient<IAppDataConfigProvider, AppDataConfigProviderUnknown>();
        services.TryAddTransient<IJsonServiceInternal, JsonServiceInternalUnknown>();
        return services;
    }

}