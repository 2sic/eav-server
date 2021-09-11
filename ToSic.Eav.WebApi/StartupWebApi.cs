using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Convert;
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
            services.TryAddTransient<QueryApi.Dependencies>();
            services.TryAddTransient<ContentExportApi>();
            services.TryAddTransient<ContentImportApi>();

            // Internal API helpers
            services.TryAddTransient<EntityApi>();
            services.TryAddTransient<MetadataBackend>();

            // WebApi Security
            services.TryAddTransient<MultiPermissionsTypes>();

            // WIP Converter clean-up v12.05
            // This is still needed on one EAV WebApi for DataSource to JsonBasic conversion
            // Here we only register the dependencies, as the final converter must be registered elsewhere
            services.TryAddTransient<ConvertToJsonLight.Dependencies>();
            services.TryAddTransient<ConvertToJsonLight, ConvertToJsonLight>();

            return services;
        }
    }
}