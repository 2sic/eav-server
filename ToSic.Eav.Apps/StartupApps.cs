using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps.AppMetadata;
using ToSic.Eav.Apps.Decorators;
using ToSic.Eav.Apps.Environment;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Apps.Languages;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Apps.Paths;
using ToSic.Eav.Apps.Run;
using ToSic.Eav.Apps.Security;
using ToSic.Eav.Context;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Repositories;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps
{
    public static class StartupApps
    {
        public static IServiceCollection AddEavApps(this IServiceCollection services)
        {
            // Context
            services.TryAddTransient<IContextOfSite, ContextOfSite>();
            services.TryAddTransient<ContextOfSite>();
            //services.TryAddTransient<ContextOfSite.ContextOfSiteDependencies>();

            // Runtimes and Managers
            services.TryAddTransient<AppRuntimeDependencies>();
            services.TryAddTransient<AppRuntime>();
            services.TryAddTransient<AppManager>();
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
            services.TryAddTransient<EntityRuntime>();

            services.TryAddTransient<Import>();

            services.TryAddTransient<ZipExport>();
            services.TryAddTransient<ZipImport>();
            services.TryAddTransient<ZipFromUrlImport>();

            // App Dependencies
            services.TryAddTransient<App.AppDependencies>();

            // File System Loaders (note: Dnn will introduce it's own to work around a DI issue)
            services.TryAddTransient<IAppFileSystemLoader, AppFileSystemLoader>();
            services.TryAddTransient<IAppRepositoryLoader, AppFileSystemLoader>();
            services.TryAddTransient<AppFileSystemLoader.Dependencies>();

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
            services.TryAddTransient<XmlImportWithFiles.Dependencies>();

            // V13 Language Checks
            services.TryAddTransient<AppUserLanguageCheck>();

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
        public static IServiceCollection AddFallbackAppServices(this IServiceCollection services)
        {
            services.TryAddTransient<IEnvironmentLogger, EnvironmentLoggerUnknown>();
            services.TryAddTransient<XmlExporter, XmlExporterUnknown>();
            services.TryAddTransient<ISite, SiteUnknown>();
            services.TryAddTransient<IZoneMapper, ZoneMapperUnknown>();
            services.TryAddTransient<AppPermissionCheck, AppPermissionCheckUnknown>();
            services.TryAddTransient<IAppFileSystemLoader, FileSystemLoaderUnknown>();

            services.TryAddTransient<IImportExportEnvironment, ImportExportEnvironmentUnknown>();

            return services;
        }

    }
}
