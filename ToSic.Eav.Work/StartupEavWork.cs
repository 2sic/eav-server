using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps.ImportExport.Sys;
using ToSic.Eav.Apps.Inspect.Sys;
using ToSic.Eav.Apps.Inspect.Sys.WebApi;
using ToSic.Eav.Apps.Install.Sys;
using ToSic.Eav.Apps.Sys.Initializers;
using ToSic.Eav.Data.ContentTypes.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.DataFormats.EavLight.Sys;
using ToSic.Eav.DataSources.ImportExport.Sys;
using ToSic.Eav.DataSources.Query.Dev.Sys;
using ToSic.Eav.DataSources.Query.Sys;
using ToSic.Eav.ImportExport.Sys.Bundle;
using ToSic.Eav.ImportExport.Sys.JsonData;
using ToSic.Eav.ImportExport.Sys.XmlData;
using ToSic.Eav.ImportExport.Sys.XmlList;
using ToSic.Eav.Metadata.Recommendations.Sys;
using ToSic.Eav.Metadata.Requirements.Sys;
using ToSic.Eav.Metadata.Sys;

namespace ToSic.Eav;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupEavWork
{
    public static IServiceCollection AddEavWork(this IServiceCollection services)
    {
        // Runtimes and Managers
        services.TryAddTransient<ZoneManager>();

        // Runtime parts
        services.TryAddTransient<RecommendedMetadataService>(); // new v13
        services.TryAddTransient<MetadataRequirementsService>(); // new v13
        services.TryAddTransient<IRequirementsService, MetadataRequirementsService>(); // new v16.08

        // v17
        services.TryAddTransient<IRequirementsService, RequirementsServiceUnknown>();

        // New part v16 with better architecture
        services.TryAddTransient<AppWorkContextService>();
        //services.TryAddTransient(typeof(GenWorkPlus<>));
        //services.TryAddTransient(typeof(GenWorkDb<>));
        //services.TryAddTransient(typeof(GenWorkBasic<>));
        services.TryAddTransient<WorkEntities>();
        services.TryAddTransient<WorkInputTypes>();
        services.TryAddTransient<WorkEntitySave>();
        services.TryAddTransient<WorkEntityCreate>();
        services.TryAddTransient<WorkEntityUpdate>();
        services.TryAddTransient<WorkMetadata>();
        services.TryAddTransient<WorkFieldList>();
        services.TryAddTransient<WorkEntityDelete>();
        services.TryAddTransient<WorkEntityRecycleBin>();
        services.TryAddTransient<WorkEntityRecycle>();
        services.TryAddTransient<WorkEntityPublish>();
        services.TryAddTransient<WorkEntityVersioning>();
        services.TryAddTransient<WorkContentTypesMod>();
        services.TryAddTransient<WorkAttributes>();
        services.TryAddTransient<WorkAttributesMod>();
        services.TryAddTransient<WorkQueryMod>();
        services.TryAddTransient<WorkQueryCopy>();
        services.TryAddTransient<WorkQueryDelete>();

        // v22 adding work App
        services.TryAddTransient<WorkAppsRemove>();

        // Helpers to build stuff
        services.TryAddTransient<AppCreator>();
        services.TryAddTransient<ZoneCreator>();
        services.TryAddTransient<IAppInitializedChecker, AppInitializedChecker>();
        services.TryAddTransient<AppInitializedChecker>();
        services.TryAddTransient<AppInitializer>();

        // v21.04 - low-code actions for content-type schema changes
        services.TryAddTransient<ContentTypeChangeActionRunner>();

        // export import stuff
        services.TryAddScoped<ExportListXml>();
        services.TryAddScoped<ImportListXml>();

        // This is still needed on one EAV WebApi for DataSource to JsonBasic conversion
        // Here we only register the dependencies, as the final converter must be registered elsewhere
        services.TryAddTransient<ConvertEavLightForCode.Dependencies>();
        services.TryAddTransient<IConvertToEavLight, ConvertEavLightForCode>();


        // v22 - Enable AppWorkCtx to be injected
        services.AddAppWorkContext();

        // v22
        services.TryAddTransient<WorkApps>();
        services.TryAddTransient<WorkFieldsDataTypes>();
        services.TryAddTransient(typeof(AppWorkQuick<>));
        services.TryAddTransient(typeof(AppWorkChain<>));

        // v22
        services.TryAddTransient<AppDetailsService>();  // registration for composition
        services.TryAddTransient<IAppDetailsService, AppDetailsService>();
        services.TryAddTransient<WorkAppsInheritable>();

        // APIs - moved here from WebApi 2026-11-12
        services.TryAddTransient<AppWebApiControllerAnalyzer>();
        services.TryAddTransient<IApiInspector, ApiInspectorUnknown>();
        services.TryAddTransient<IAppExplorerControllerDependency, AppExplorerControllerDependencyUnknown>();
        services.TryAddTransient<QueryGetDtoWip>();

        // New v22
        services.TryAddTransient<ConvertEntityToEavLight>();
        services.TryAddTransient<WorkQueryImport>();
        services.TryAddTransient<WorkQuerySave>();
        services.TryAddTransient<QueryDevSvc>();
        services.TryAddTransient<QueryDevDebugSvc>();
        services.TryAddTransient<WorkTypePermissions>();

        // v22 moving here...
        services.TryAddTransient<EntitiesAdminData>();

        // basic environment, pages, modules etc.
        // Note that it's not really part of .Web, but we want it to be quite late so we don't need
        // to move up dependencies which it has.
        services.TryAddTransient<IPlatformAppInstaller, PlatformAppInstallerUnknown>();
        services.TryAddTransient<PendingAppsGet>();


        // Moving to Eav.Work
        services.TryAddTransient<ExportBundleFile>();
        services.TryAddTransient<ExportDataXml>();
        services.TryAddTransient<ExportTypeJson>();
        services.TryAddTransient<ExportEntityJson>();
        services.TryAddTransient<ExportBundlePrepare>();
        services.TryAddTransient<ExportBundleSave>();
        services.TryAddTransient<ImportDataJson>();
        services.TryAddTransient<ImportDataXml>();
        services.TryAddTransient<ImportDataXmlPreview>();

        // Converters, new v16
        services.TryAddTransient<ConvertAttributeToRaw>();
        services.TryAddTransient<ConvertContentTypeToRaw>();

        services.TryAddTransient<ImportAppService>();
        services.TryAddTransient<PendingAppInstall>();

        // v22 App moved here from Sxc
        // Note: most of it should probably go to ToSic.Eav.Work, but this can't be done yet, because of WebApi/Http dependencies
        services.TryAddTransient<ExportApp>();
        services.TryAddTransient<ExportHelper>();
        services.TryAddTransient<ImpExpHelpers>();
        services.TryAddTransient<AppStateSyncSave>();   // new separate in v21.06
        services.TryAddTransient<AppStateSyncRestore>();
        services.TryAddTransient<ImportFromRemote>();

        services.TryAddTransient<WorkMetadataGet>();

        return services;
    }

    /// <summary>
    /// This will add Do-Nothing services which will take over if they are not provided by the main system
    /// In general this will result in some features missing, which many platforms don't need or care about
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    /// <remarks>
    /// All calls in here MUST use TryAddTransient, and never without the Try
    /// </remarks>
    public static IServiceCollection AddWorkFallbackServices(this IServiceCollection services)
    {
        return services;
    }
    
    /// <summary>
    /// New / WIP v22
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddAppWorkContext(this IServiceCollection services)
    {
        // Make sure that when nothing was overridden, we will get an exception, so that we don't accidentally use a missing context
        services.TryAddTransient<AppWorkContextNotReplaced>();
        services.TryAddTransient(OverrideService<IAppWorkContext>.Register<AppWorkContextNotReplaced>());

        return services;
    }

}
