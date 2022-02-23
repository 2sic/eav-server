using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.WebApi.ApiExplorer;
using ToSic.Eav.WebApi.Features;
using ToSic.Eav.WebApi.ImportExport;
using ToSic.Eav.WebApi.Languages;
using ToSic.Eav.WebApi.Licenses;
using ToSic.Eav.WebApi.Plumbing;
using ToSic.Eav.WebApi.Security;
using ToSic.Eav.WebApi.Sys;
using ToSic.Eav.WebApi.Zone;

namespace ToSic.Eav.WebApi
{
    public static class StartupWebApi
    {
        public static IServiceCollection AddEavWebApi(this IServiceCollection services)
        {
            // Insights, the most important core backend
            services.TryAddTransient<InsightsControllerReal>();

            // Various Backends
            services.TryAddTransient<FeaturesBackend>();
            services.TryAddTransient<LicenseBackend>();
            services.TryAddTransient<LanguagesBackend>();
            services.TryAddTransient<ZoneBackend>();


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
            services.TryAddTransient<ConvertToEavLight.Dependencies>();
            services.TryAddTransient<ConvertToEavLight, ConvertToEavLight>();

            return services;
        }

        public static IServiceCollection AddEavApiExplorer<THttpResponseType>(this IServiceCollection services)
        {
            // APIs
            services.TryAddTransient<ApiExplorerBackend<THttpResponseType>>();
            services.TryAddTransient<IApiInspector, ApiInspectorUnknown>();
            services.TryAddTransient<ResponseMaker<THttpResponseType>, ResponseMakerUnknown<THttpResponseType>>();
            return services;
        }
    }
}