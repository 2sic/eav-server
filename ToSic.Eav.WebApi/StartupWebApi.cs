using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps.ImportExport.Sys;
using ToSic.Eav.Data.ContentTypes.Sys;
using ToSic.Eav.Metadata.Sys;
using ToSic.Eav.Ui.Context.Sys;
using ToSic.Eav.Web.Sys;
using ToSic.Eav.WebApi.Sys;
using ToSic.Eav.WebApi.Sys.Admin;
using ToSic.Eav.WebApi.Sys.Admin.Features;
using ToSic.Eav.WebApi.Sys.Cms;
using ToSic.Eav.WebApi.Sys.Entities;
using ToSic.Eav.WebApi.Sys.Helpers.Http;
using ToSic.Eav.WebApi.Sys.Helpers.Json;
using ToSic.Eav.WebApi.Sys.ImportExport;
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
        services.TryAddTransient<WorkMetadataGet>();
        services.TryAddTransient<EntityControllerReal>();
        services.TryAddTransient<FieldControllerReal>();
        services.TryAddTransient<ZoneControllerReal>();
        services.TryAddTransient<LogControllerReal>();
        services.TryAddTransient<LicenseControllerReal>();

        // Various Backends
        services.TryAddTransient<SaveEntities>();

        // APIs
        services.TryAddTransient<EntityPickerApi>();
        services.TryAddTransient<ContentTypeDtoService>();
        services.TryAddTransient<ContentExportApi>();
        services.TryAddTransient<ContentImportApi>();

        // Converters, new v16
        services.TryAddTransient<ConvertAttributeToRaw>();
        services.TryAddTransient<ConvertContentTypeToRaw>();

        // Internal API helpers
        services.TryAddTransient<EditLoadActionGetForEditing>(); // WIP v21
        services.TryAddTransient<ContextFeaturesSvc>();
        services.TryAddTransient<ImportAppService>();
        services.TryAddTransient<PendingAppInstall>();

        // json serialization converters eav related
        services.TryAddTransient<EavJsonConverter>();
        services.TryAddTransient<EavCollectionJsonConverter>();
        services.TryAddTransient<EavJsonConverterFactory>();

        // ResponseMaker must be scoped, as the api-controller must init this for use in other parts
        // This ensures that generic backends (.net framework/core) can create a response object
#if NETCOREAPP
        // In .net core, it does not need to be initialized in the controller.
        services.TryAddScoped<IResponseMaker, ResponseMakerNetCore>();
#else
        // The initial dummy, which will always throw an exception if used. This is to ensure that the ResponseMaker is always overridden in the EAV WebApi.
        services.TryAddTransient<ResponseMakerToOverride>();
        // The true registration for the IResponseMaker, requiring override.
        services.TryAddTransient<IResponseMaker>(OverrideService<IResponseMaker>.Register<ResponseMakerToOverride>());
#endif

        // Helper to decouple Insights from WebApi
        services.TryAddTransient<IHttpExceptionMaker, HttpExceptionMaker>();

        // v20 centralize log creation / access for an http request
        services.TryAddScoped<HttpRequestLoggingScoped>();

        // v22 Context
        services.TryAddTransient<IUiContextService, UiContextService>();  // v22 - default implementation, for re-implementations
        services.TryAddTransient<EavUiContextPartsService>();
        services.TryAddTransient<IUiContextPartsService, EavUiContextPartsService>();

        // v22 App moved here from Sxc
        // Note: most of it should probably go to ToSic.Eav.Work, but this can't be done yet, because of WebApi/Http dependencies
        services.TryAddTransient<ExportApp>();
        services.TryAddTransient<ExportHelper>();
        services.TryAddTransient<ImpExpHelpers>();
        services.TryAddTransient<AppStateSyncSave>();   // new separate in v21.06
        services.TryAddTransient<AppStateSyncRestore>();
        services.TryAddTransient<ImportFromRemote>();


        services.AddNetInfrastructure();

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
        services.TryAddScoped<IResponseMaker, ResponseMakerToOverride>();
        return services;
    }

    private static IServiceCollection AddNetInfrastructure(this IServiceCollection services)
    {
#if NETCOREAPP
        // Helper to get header, query string and route information from current request
        services.TryAddScoped<RequestHelper>();
#endif

        return services;
    }

}
