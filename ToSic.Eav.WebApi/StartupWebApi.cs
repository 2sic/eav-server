using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Context.Sys;
using ToSic.Eav.Ui.Context.Sys;
using ToSic.Eav.Ui.Edit.Sys;
using ToSic.Eav.Ui.Edit.Sys.Load.Activities;
using ToSic.Eav.Ui.Edit.Sys.Load.Links;
using ToSic.Eav.Ui.Edit.Sys.Load.Prefetch;
using ToSic.Eav.Ui.Edit.Sys.Load.Settings;
using ToSic.Eav.Ui.Edit.Sys.Save;
using ToSic.Eav.Ui.History.Sys;
using ToSic.Eav.Web.Http.Sys;
using ToSic.Eav.WebApi.Admin.Sys;
using ToSic.Eav.WebApi.App.Sys;
using ToSic.Eav.WebApi.OData.Sys;
using ToSic.Eav.WebApi.Sys.Helpers.Json;
using ToSic.Eav.WebApi.Sys.Licenses;
using ToSic.Eav.WebApi.Sys.Logs;

namespace ToSic.Eav;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
public static class StartupWebApi
{
    public static IServiceCollection AddEavWebApi(this IServiceCollection services)
    {
        // Real Controller Implementations https://go.2sxc.org/proxy-controllers
        services.TryAddTransient<FeatureControllerReal>();
        services.TryAddTransient<EntityControllerReal>();
        services.TryAddTransient<FieldControllerReal>();
        services.TryAddTransient<ZoneControllerReal>();
        services.TryAddTransient<LogControllerReal>();
        services.TryAddTransient<LicenseControllerReal>();
        services.TryAddTransient<HistoryControllerReal>();

        // Various Backends
        services.TryAddTransient<SaveHelperSaveBundles>();
        services.TryAddTransient<SaveHelperGetFinalIds>();

        // APIs
        services.TryAddTransient<EditPrefetchEntitiesSvc>();


        // Internal API helpers
        services.TryAddTransient<EditLoadActionGetForEditing>(); // WIP v21
        services.TryAddTransient<ContextFeaturesSvc>();

        // json serialization converters eav related
        services.TryAddTransient<EavJsonConverter>();
        services.TryAddTransient<EavCollectionJsonConverter>();
        services.TryAddTransient<EavJsonConverterFactory>();

        // ResponseMaker must be scoped, as the api-controller must init this for use in other parts
        // This ensures that generic backends (.net framework/core) can create a response object
//#if NETCOREAPP
        // In .net core, it does not need to be initialized in the controller.
        //services.TryAddScoped<IResponseMaker, ResponseMakerNetCore>();
//#else
        // The initial dummy, which will always throw an exception if used. This is to ensure that the ResponseMaker is always overridden in the EAV WebApi.
        //services.TryAddTransient<ResponseMakerToOverride>();
        // The true registration for the IResponseMaker, requiring override.
        //services.TryAddTransient<IResponseMaker>(OverrideService<IResponseMaker>.Register<ResponseMakerToOverride>());
//#endif

        // v20 centralize log creation / access for an http request
        services.TryAddScoped<HttpRequestLoggingScoped>();

        // v22 Context
        services.TryAddTransient<IWebApiAppContextBuilderService, WebApiAppContextBuilderService>();
        services.TryAddTransient<IUiContextService, UiContextService>();  // v22 - default implementation, for re-implementations
        services.TryAddTransient<EavUiContextPartsService>();
        services.TryAddTransient<IUiContextPartsService, EavUiContextPartsService>();

        // Moved here v22 (previously Sxc)
        services.TryAddTransient<AppWebApiQueryRunSvc>();
        services.TryAddTransient<AppQueryODataHelper>();

        services.AddEavWebApiEdit();

        services.AddNetInfrastructure();

        return services;
    }


    public static IServiceCollection AddEavWebApiEdit(this IServiceCollection services)
    {
        // Edit Load Identity Providers
        services.TryAddTransient<EditLoadIdentifierProviders>();
        services.AddTransient<IEditLoadIdentifierProvider, EditLoadIdentifierProviderMetadataSingleton>();

        // Prefetch Edit Load
        services.TryAddKeyedTransient<IEditLoadPrefetchProvider, EditLoadActivityPrefetchEntity>(EditLoadActivityPrefetchEntity.NameId);
        services.TryAddKeyedTransient<IEditLoadPrefetchProvider, EditLoadActivityPrefetchLinks>(EditLoadActivityPrefetchLinks.NameId);

        // Edit Load Helpers / Activities
        services.TryAddTransient<EditLoadActivityAddContentTypes>();
        services.TryAddTransient<EditLoadActivityAddNecessaryInputTypes>();
        services.TryAddTransient<EditLoadActivityAddContext>();
        services.TryAddTransient<EditLoadActivityAddRequiredFeatures>();
        services.TryAddTransient<EditLoadActivityAddPrefetch>();
        services.TryAddTransient<EditLoadActivitySettingsHelper>();
        services.TryAddTransient<EditLoadActivityConvertRequest>();

        // Load Settings Providers
        services.AddTransient<ILoadSettingsContentTypesProvider, LoadSettingsForPickerSources>();
        services.AddTransient<ILoadSettingsProvider, LoadSettingsForContentType>();

        // SaveData PreSave
        services.TryAddTransient<DataValidatorContentTypeDataStore>();


        return services;
    }

    /// <summary>
    /// Add typed EAV WebApi objects.
    /// Make sure it's called AFTER adding the normal EAV. Otherwise, it takes precedence.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddEavWebApiTypedAfterEav(this IServiceCollection services)
    {
        // The ResponseMaker must be registered as generic, so that any specific registration will have priority
        //services.TryAddScoped<IResponseMaker, ResponseMakerToOverride>();
        return services;
    }

    public static IServiceCollection AddEavWebApiFallbacks(this IServiceCollection services)
    {
        services.TryAddTransient<ILinkResolver, LinkResolverUnknown>();
        return services;
    }

    private static IServiceCollection AddNetInfrastructure(this IServiceCollection services)
    {
#if NETCOREAPP
        // Helper to get header, query string and route information from current request
        services.TryAddScoped<ToSic.Eav.WebApi.Sys.Helpers.Http.RequestHelper>();
        services.TryAddTransient<IHttp, HttpHybrid>();
#endif

        return services;
    }

}
