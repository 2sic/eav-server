using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Api.Api01;
using ToSic.Eav.Apps.Decorators;
using ToSic.Eav.Apps.Environment;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Apps.Languages;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Apps.Paths;
using ToSic.Eav.Apps.Run;
using ToSic.Eav.Apps.Security;
using ToSic.Eav.Context;
using ToSic.Eav.DataSources.Catalog;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Repositories;
using ToSic.Eav.Run;
using ToSic.Eav.Security;

namespace ToSic.Eav.Apps
{
    public static class StartupApps
    {
        public static IServiceCollection AddEavApps(this IServiceCollection services)
        {
            // Context
            services.TryAddTransient<IContextOfSite, ContextOfSite>();
            services.TryAddTransient<ContextOfSite>();

            // Runtimes and Managers
            services.TryAddTransient<AppRuntimeBase.MyServices>();
            services.TryAddTransient<AppRuntime>();
            services.TryAddTransient<AppManager>();
            services.TryAddTransient<AppManager.MyServices>();
            services.TryAddTransient<ZoneRuntime>();
            services.TryAddTransient<ZoneManager>();
            services.TryAddTransient<QueryManager>();
            services.TryAddTransient<SystemManager>();
            services.TryAddTransient<AppFinder>();

            // Runtime parts
            services.TryAddTransient<ContentTypeRuntime>();
            services.TryAddTransient<QueryRuntime>();
            services.TryAddTransient<MetadataRuntime>();
            services.TryAddTransient<MdRecommendations>(); // new v13
            services.TryAddTransient<MdRequirements>(); // new v13
            services.TryAddTransient<EntityRuntime>();
            services.TryAddTransient<EntitiesManager>();

            services.TryAddTransient<Import>();

            services.TryAddTransient<ZipExport>();
            services.TryAddTransient<ZipImport>();
            services.TryAddTransient<ZipImport.MyServices>();
            services.TryAddTransient<ZipFromUrlImport>();

            // App Dependencies
            services.TryAddTransient<App.MyServices>();

            // Context
            services.TryAddTransient<IContextOfApp, ContextOfApp>();
            services.TryAddTransient<ContextOfApp.MyServices>();
            services.TryAddTransient<ContextOfSite.MyServices>();


            // File System Loaders (note: Dnn will introduce it's own to work around a DI issue)
            services.TryAddTransient<IAppFileSystemLoader, AppFileSystemLoader>();
            services.TryAddTransient<IAppRepositoryLoader, AppFileSystemLoader>();
            services.TryAddTransient<AppFileSystemLoader.MyServices>();

            // Helpers to build stuff
            services.TryAddTransient<AppCreator>();
            services.TryAddTransient<ZoneCreator>();
            services.TryAddTransient<IAppInitializedChecker, AppInitializedChecker>();
            services.TryAddTransient<AppInitializedChecker>();
            services.TryAddTransient<AppInitializer>();
            services.TryAddTransient<AppPaths>();

            // export import stuff
            services.TryAddScoped<ExportListXml>();
            services.TryAddScoped<ImportListXml>();
            services.TryAddTransient<ExportImportValueConversion>();
            services.TryAddTransient<XmlImportWithFiles.MyServices>();

            // Simple DataController - registration was missing
            services.TryAddTransient<SimpleDataController>();

            // App Permission Check moved to this project as the implementations are now all identical
            services.TryAddTransient<AppPermissionCheck>();
            services.TryAddTransient<MultiPermissionsTypes>();
            services.TryAddTransient<MultiPermissionsApp>();
            services.TryAddTransient<MultiPermissionsApp.MyServices>();

            // V13 Language Checks
            services.TryAddTransient<AppUserLanguageCheck>();

            // V15 Data Sources
            services.TryAddTransient<Eav.DataSources.CustomDataSourceAdvanced.MyServices>();


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
            services.TryAddTransient<IAppFileSystemLoader, FileSystemLoaderUnknown>();
            services.TryAddTransient<IImportExportEnvironment, ImportExportEnvironmentUnknown>();

            return services;
        }

    }
}
