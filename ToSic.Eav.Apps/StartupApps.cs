using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

            // export import stuff
            services.TryAddTransient<ExportImportValueConversion>();

            return services;
        }

    }
}
