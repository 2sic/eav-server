using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.WebApi.Security;

namespace ToSic.Eav.WebApi
{
    public static class StartupWebApi
    {
        public static IServiceCollection AddEavWebApi(this IServiceCollection services)
        {
            // APIs
            services.TryAddTransient<EntityPickerApi>();
            services.TryAddTransient<ContentTypeApi>();
            services.TryAddTransient<QueryApi>();
            services.TryAddTransient<QueryApi.Dependencies>();
            services.TryAddTransient<ContentExportApi>();
            services.TryAddTransient<ContentImportApi>();

            // Internal API helpers
            services.TryAddTransient<EntityApi>();
            services.TryAddTransient<MetadataBackend>();

            // WebApi Security
            services.TryAddTransient<MultiPermissionsTypes>();


            return services;
        }
    }
}