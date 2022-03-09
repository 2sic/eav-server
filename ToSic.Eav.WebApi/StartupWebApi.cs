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
            services.TryAddTransient(typeof(EntityControllerReal<>));
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
            services.TryAddTransient(typeof(ContentExportApi<>));
            services.TryAddTransient<ContentImportApi>();

            // Internal API helpers
            services.TryAddTransient<EntityApi>();

            // WIP Converter clean-up v12.05
            // This is still needed on one EAV WebApi for DataSource to JsonBasic conversion
            // Here we only register the dependencies, as the final converter must be registered elsewhere
            services.TryAddTransient<ConvertToEavLight.Dependencies>();
            services.TryAddTransient<ConvertToEavLight, ConvertToEavLight>();


#if NETFRAMEWORK
            services.TryAddScoped<ResponseMaker<HttpResponseMessage>, ResponseMakerNetFramework>(); // must be scoped, as the api-controller must init this for use in other parts
#endif

            return services;
        }

        /// <summary>
        /// Add typed EAV WebApi objects.
        /// Make sure it's called AFTER adding the normal EAV. Otherwise the ResponseMaker will be the unknown even in
        /// </summary>
        /// <typeparam name="THttpResponseType"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddEavWebApiTypedAfterEav<THttpResponseType>(this IServiceCollection services)
        {
            // APIs
            services.TryAddTransient<ApiExplorerControllerReal<THttpResponseType>>();
            services.TryAddTransient<IApiInspector, ApiInspectorUnknown>();
            // The ResponseMaker must be registered as generic, so that any specific registration will have priority
            services.TryAddScoped(typeof(ResponseMaker<>), typeof(ResponseMakerUnknown<>));
            return services;
        }
    }
}