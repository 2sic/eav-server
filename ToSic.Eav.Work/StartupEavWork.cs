using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps.Internal.Work;
using ToSic.Eav.ImportExport.Internal.XmlList;
using ToSic.Metadata.Recommendations.Sys;
using ToSic.Metadata.Requirements.Sys;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Apps;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupEavWork
{
    public static IServiceCollection AddEavWork(this IServiceCollection services)
    {
        //// Context
        //services.TryAddTransient<IContextOfSite, ContextOfSite>();
        //services.TryAddTransient<ContextOfSite>();

        // Runtimes and Managers
        services.TryAddTransient<ZoneManager>();
        //services.TryAddTransient<AppCachePurger>();
        //services.TryAddTransient<AppFinder>();

        // Runtime parts
        services.TryAddTransient<RecommendedMetadataService>(); // new v13
        services.TryAddTransient<MetadataRequirementsService>(); // new v13
        services.TryAddTransient<IRequirementsService, MetadataRequirementsService>(); // new v16.08

        // v17
        services.TryAddTransient<IRequirementsService, RequirementsServiceUnknown>();

        // New part v16 with better architecture
        services.TryAddTransient<AppWorkContextService>();
        services.TryAddTransient(typeof(GenWorkPlus<>));
        services.TryAddTransient(typeof(GenWorkDb<>));
        services.TryAddTransient(typeof(GenWorkBasic<>));
        services.TryAddTransient<WorkEntities>();
        services.TryAddTransient<WorkInputTypes>();
        services.TryAddTransient<WorkEntitySave>();
        services.TryAddTransient<WorkEntityCreate>();
        services.TryAddTransient<WorkEntityUpdate>();
        services.TryAddTransient<WorkMetadata>();
        services.TryAddTransient<WorkFieldList>();
        services.TryAddTransient<WorkEntityDelete>();
        services.TryAddTransient<WorkEntityPublish>();
        services.TryAddTransient<WorkEntityVersioning>();
        services.TryAddTransient<WorkContentTypesMod>();
        services.TryAddTransient<WorkAttributes>();
        services.TryAddTransient<WorkAttributesMod>();
        services.TryAddTransient<WorkQueryMod>();
        services.TryAddTransient<WorkQueryCopy>();

        // Helpers to build stuff
        services.TryAddTransient<AppCreator>();
        services.TryAddTransient<ZoneCreator>();
        services.TryAddTransient<IAppInitializedChecker, AppInitializedChecker>();
        services.TryAddTransient<AppInitializedChecker>();
        services.TryAddTransient<AppInitializer>();



        // export import stuff
        services.TryAddScoped<ExportListXml>();
        services.TryAddScoped<ImportListXml>();


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

}