using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.WebApi.Admin;
using ToSic.Eav.WebApi.Admin.Features;
using ToSic.Eav.WebApi.Admin.Metadata;
using ToSic.Eav.WebApi.Admin.Query;
using ToSic.Eav.WebApi.ApiExplorer;
using ToSic.Eav.WebApi.Cms;
using ToSic.Eav.WebApi.ImportExport;
using ToSic.Eav.WebApi.Infrastructure;
using ToSic.Eav.WebApi.Languages;
using ToSic.Eav.WebApi.SaveHelpers;
using ToSic.Eav.WebApi.Serialization;
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

            // Real Controller Implementations https://go.2sxc.org/proxy-controllers
            services.TryAddTransient<FeatureControllerReal>();
            services.TryAddTransient<MetadataControllerReal>();
            services.TryAddTransient(typeof(EntityControllerReal<>));
            services.TryAddTransient<FieldControllerReal>();
            services.TryAddTransient<ZoneControllerReal>();
            services.TryAddTransient<LogControllerReal>();
            services.TryAddTransient<LicenseControllerReal>();

            // Various Backends
            services.TryAddTransient<LanguagesBackend>();
            services.TryAddTransient<ZoneBackend>();
            services.TryAddTransient<SaveEntities>();

            // APIs
            services.TryAddTransient<EntityPickerApi>();
            services.TryAddTransient<ContentTypeApi>();
            services.TryAddTransient(typeof(QueryControllerBase<>.MyServices));
            services.TryAddTransient(typeof(ContentExportApi<>));
            services.TryAddTransient<ContentImportApi>();

            // Internal API helpers
            services.TryAddTransient<EntityApi>();
            services.TryAddTransient<IUiData, UiData>();

            // WIP Converter clean-up v12.05
            // This is still needed on one EAV WebApi for DataSource to JsonBasic conversion
            // Here we only register the dependencies, as the final converter must be registered elsewhere
            services.TryAddTransient<ConvertToEavLight.MyServices>();
            services.TryAddTransient<ConvertToEavLight, ConvertToEavLight>();

            // json serialization converters eav related
            services.TryAddTransient<EavJsonConverter>();
            services.TryAddTransient<EavCollectionJsonConverter>();
            services.TryAddTransient<EavJsonConverterFactory>();

            services.AddNetInfrastructure();

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

#if NETFRAMEWORK
        public static IServiceCollection AddNetInfrastructure(this IServiceCollection services)
        {
            // ResponseMaker must be scoped, as the api-controller must init this for use in other parts
            services.TryAddScoped<ResponseMaker>();     // this must come first!
            services.TryAddScoped<ResponseMaker<System.Net.Http.HttpResponseMessage>>(x => x.GetRequiredService<ResponseMaker>());

            return services;
        }
#else
        public static IServiceCollection AddNetInfrastructure(this IServiceCollection services)
        {
            // Helper to get header, query string and route information from current request
            services.TryAddScoped<RequestHelper>();

            // ResponseMaker must be scoped, as the api-controller must init this for use in other parts
            // This ensures that generic backends (.net framework/core) can create a response object
            services.TryAddScoped<ResponseMaker>();     // this must come first!
            services.TryAddScoped<ResponseMaker<Microsoft.AspNetCore.Mvc.IActionResult>>(x => x.GetRequiredService<ResponseMaker>());

            //// This ensures that generic backends (.net framework/core) can create a response object
            //services.TryAddScoped<ResponseMaker<Microsoft.AspNetCore.Mvc.IActionResult>, ResponseMakerNetCore>();

            return services;
        }
#endif

    }
}