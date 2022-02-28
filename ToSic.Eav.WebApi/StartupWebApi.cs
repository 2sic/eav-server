using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.WebApi.Admin;
using ToSic.Eav.WebApi.Admin.Features;
using ToSic.Eav.WebApi.Admin.Metadata;
using ToSic.Eav.WebApi.Admin.Query;
using ToSic.Eav.WebApi.ApiExplorer;
using ToSic.Eav.WebApi.ImportExport;
using ToSic.Eav.WebApi.Languages;
using ToSic.Eav.WebApi.Plumbing;
using ToSic.Eav.WebApi.Security;
using ToSic.Eav.WebApi.Sys;
using ToSic.Eav.WebApi.Sys.Licenses;
using ToSic.Eav.WebApi.Zone;

namespace ToSic.Eav.WebApi
{
    public static class StartupWebApi
    {
        public static IServiceCollection AddEavWebApi(this IServiceCollection services)
        {
            // Insights, the most important core backend
            services.TryAddTransient<InsightsControllerReal>();
            services.TryAddSingleton<DummyControllerReal>();

            // Real Controller Implementations https://r.2sxc.org/proxy-controllers
            services.TryAddTransient<FeatureControllerReal>();
            services.TryAddTransient<MetadataControllerReal>();
            services.TryAddTransient<EntityControllerReal>();
            services.TryAddTransient<FieldControllerReal>();
            services.TryAddTransient<ZoneControllerReal>();
            services.TryAddTransient<LicenseControllerReal>();
            services.TryAddTransient<LogControllerReal>();

            // Various Backends
            services.TryAddTransient<LanguagesBackend>();
            services.TryAddTransient<ZoneBackend>();


            // APIs
            services.TryAddTransient<EntityPickerApi>();
            services.TryAddTransient<ContentTypeApi>();
            services.TryAddTransient<QueryControllerDependencies>();
            services.TryAddTransient<ContentExportApi>();
            services.TryAddTransient<ContentImportApi>();

            // Internal API helpers
            services.TryAddTransient<EntityApi>();

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