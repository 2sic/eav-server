using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps.Environment;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Apps.Parts;

namespace ToSic.Eav.Apps
{
    public static class StartupApps
    {
        public static IServiceCollection AddEavApps(this IServiceCollection services)
        {
            // Runtimes and Managers
            services.TryAddTransient<AppRuntime>();
            services.TryAddTransient<AppManager>();
            services.TryAddTransient<ZoneRuntime>();
            services.TryAddTransient<ZoneManager>();

            // Runtime parts
            services.TryAddTransient<ContentTypeRuntime>();
            services.TryAddTransient<QueryRuntime>();
            services.TryAddTransient<MetadataRuntime>();
            services.TryAddTransient<EntityRuntime>();

            services.TryAddTransient<Import>();

            services.TryAddTransient<ZipExport>();
            services.TryAddTransient<ZipImport>();
            services.TryAddTransient<ZipFromUrlImport>();

            // App Dependencies
            services.TryAddTransient<App.AppDependencies>();

            // Helpers to build stuff
            services.TryAddTransient<AppCreator>();
            services.TryAddTransient<ZoneCreator>();
            services.TryAddTransient<IAppInitializedChecker, AppInitializedChecker>();
            services.TryAddTransient<AppInitializedChecker>();

            // export import stuff
            services.TryAddScoped<ExportListXml>();
            services.TryAddScoped<ImportListXml>();
            services.TryAddTransient<ExportImportValueConversion>();

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
            services.TryAddTransient<IEnvironmentLogger, BasicEnvironmentLogger>();
            services.TryAddTransient<XmlExporter, BasicXmlExporter>();

            return services;
        }

    }
}
