using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps.ImportExport;

namespace ToSic.Eav.Apps
{
    public static class AppsStartup
    {
        public static IServiceCollection AddApps(this IServiceCollection services)
        {
            services.TryAddTransient<ZipExport>();
            services.TryAddTransient<ZipImport>();
            services.TryAddTransient<ZipFromUrlImport>();

            return services;
        }

    }
}
