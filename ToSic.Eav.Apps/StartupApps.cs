using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Api.Api01;
using ToSic.Eav.Apps.Decorators;
using ToSic.Eav.Apps.Environment;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Apps.Languages;
using ToSic.Eav.Apps.Paths;
using ToSic.Eav.Apps.Run;
using ToSic.Eav.Apps.Security;
using ToSic.Eav.Apps.Work;
using ToSic.Eav.Context;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Repositories;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Requirements;
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
