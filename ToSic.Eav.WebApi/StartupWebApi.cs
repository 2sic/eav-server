using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.WebApi.Sys;
using ToSic.Eav.WebApi.Sys.Admin;
using ToSic.Eav.WebApi.Sys.Admin.Features;
using ToSic.Eav.WebApi.Sys.Admin.Metadata;
using ToSic.Eav.WebApi.Sys.Admin.Query;
using ToSic.Eav.WebApi.Sys.ApiExplorer;
using ToSic.Eav.WebApi.Sys.Cms;
using ToSic.Eav.WebApi.Sys.Entities;
using ToSic.Eav.WebApi.Sys.Helpers.Http;
using ToSic.Eav.WebApi.Sys.Helpers.Json;
using ToSic.Eav.WebApi.Sys.ImportExport;
using ToSic.Eav.WebApi.Sys.Languages;
using ToSic.Eav.WebApi.Sys.Licenses;
using ToSic.Eav.WebApi.Sys.Logs;
using ToSic.Eav.WebApi.Sys.Zone;

namespace ToSic.Eav;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupWebApi
{
    public static IServiceCollection AddEavWebApi(this IServiceCollection services)
    {
        // Real Controller Implementations https://go.2sxc.org/proxy-controllers
        services.TryAddTransient<FeatureControllerReal>();
        services.TryAddTransient<MetadataControllerReal>();
        services.TryAddTransient<EntityControllerReal>();
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
        services.TryAddTransient<ContentTypeDtoService>();
        services.TryAddTransient(typeof(QueryControllerBase<>.MyServices));
        services.TryAddTransient<ContentExportApi>();
        services.TryAddTransient<ContentImportApi>();

        // Converters, new v16
        services.TryAddTransient<ConvertAttributeToDto>();
        services.TryAddTransient<ConvertContentTypeToDto>();

        // Internal API helpers
        services.TryAddTransient<EntityApi>();
        services.TryAddTransient<IUiData, UiData>();

        // WIP Converter clean-up v12.05
        // This is still needed on one EAV WebApi for DataSource to JsonBasic conversion
        // Here we only register the dependencies, as the final converter must be registered elsewhere
        services.TryAddTransient<ConvertToEavLight.MyServices>();
        services.TryAddTransient<ConvertToEavLight>();

        // json serialization converters eav related
        services.TryAddTransient<EavJsonConverter>();
        services.TryAddTransient<EavCollectionJsonConverter>();
        services.TryAddTransient<EavJsonConverterFactory>();

        // ResponseMaker must be scoped, as the api-controller must init this for use in other parts
        // This ensures that generic backends (.net framework/core) can create a response object
        services.TryAddScoped<IResponseMaker, ResponseMaker>();

        services.AddNetInfrastructure();

        return services;
    }


    /// <summary>
    /// Add typed EAV WebApi objects.
    /// Make sure it's called AFTER adding the normal EAV. Otherwise the ResponseMaker will be the unknown even in
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddEavWebApiTypedAfterEav(this IServiceCollection services)
    {
        // APIs
        services.TryAddTransient<ApiExplorerControllerReal>();
        services.TryAddTransient<IApiInspector, ApiInspectorUnknown>();
        services.TryAddTransient<IAppExplorerControllerDependency, AppExplorerControllerDependencyUnknown>();
        // The ResponseMaker must be registered as generic, so that any specific registration will have priority
        services.TryAddScoped<IResponseMaker, ResponseMakerUnknown>();
        return services;
    }

    public static IServiceCollection AddNetInfrastructure(this IServiceCollection services)
    {
#if NETCOREAPP
        // Helper to get header, query string and route information from current request
        services.TryAddScoped<RequestHelper>();
#endif

        return services;
    }

}