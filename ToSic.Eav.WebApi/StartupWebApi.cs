using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps.Internal.Insights;
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
using ToSic.Eav.WebApi.Sys.Insights;
using ToSic.Eav.WebApi.Sys.Licenses;
using ToSic.Eav.WebApi.Zone;
using InsightsControllerReal = ToSic.Eav.WebApi.Sys.Insights.InsightsControllerReal;

namespace ToSic.Eav.WebApi;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupWebApi
{
    public static IServiceCollection AddEavWebApi(this IServiceCollection services)
    {
        // Insights, the most important core backend
        services.AddInsights();

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

    public static IServiceCollection AddInsights(this IServiceCollection services)
    {
        // Insights, the most important core backend
        services.TryAddTransient<InsightsControllerReal>();
        services.TryAddTransient<InsightsDataSourceCache>();

        services.AddTransient<IInsightsProvider, InsightsIsAlive>();
        services.AddTransient<IInsightsProvider, InsightsTypes>();
        services.AddTransient<IInsightsProvider, InsightsGlobalTypes>();
        services.AddTransient<IInsightsProvider, InsightsMemoryCache>();

        services.AddTransient<IInsightsProvider, InsightsLogs>();
        services.AddTransient<IInsightsProvider, InsightsPauseLogs>();
        services.AddTransient<IInsightsProvider, InsightsLogsFlush>();
        services.AddTransient<IInsightsProvider, InsightsAppStats>();
        services.AddTransient<IInsightsProvider, InsightsPurgeApp>();
        services.AddTransient<IInsightsProvider, InsightsAppsCache>();
        services.AddTransient<IInsightsProvider, InsightsAppLoadLog>();

        services.AddTransient<IInsightsProvider, InsightsGlobalTypesLog>();
        services.AddTransient<IInsightsProvider, InsightsTypeMetadata>();
        services.AddTransient<IInsightsProvider, InsightsTypePermissions>();

        services.AddTransient<IInsightsProvider, InsightsLicenses>();
        services.AddTransient<IInsightsProvider, InsightsAttributes>();
        services.AddTransient<IInsightsProvider, InsightsAttributeMetadata>();
        services.AddTransient<IInsightsProvider, InsightsAttributePermissions>();

        services.AddTransient<IInsightsProvider, InsightsEntity>();
        services.AddTransient<IInsightsProvider, InsightsEntities>();
        services.AddTransient<IInsightsProvider, InsightsEntityPermissions>();
        services.AddTransient<IInsightsProvider, InsightsEntityMetadata>();

        services.AddTransient<IInsightsProvider, InsightsHelp>();

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