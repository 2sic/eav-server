using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.DataSource.Query.Sys;
using ToSic.Eav.DataSource.Sys.AppDataSources;
using ToSic.Eav.DataSource.Sys.Caching;
using ToSic.Eav.DataSource.Sys.Catalog;
using ToSic.Eav.DataSource.Sys.Configuration;
using ToSic.Eav.DataSources;
using ToSic.Eav.Services;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Run.Startup;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
public static class StartupEavDataSource
{
    public static IServiceCollection AddDataSourceSystem(this IServiceCollection services)
    {
        // Dependencies, new in v15
        services.TryAddTransient<DataSourceBase.Dependencies>();
        services.TryAddTransient<IDataSourceConfiguration, DataSourceConfiguration>();
        services.TryAddTransient<DataSourceConfiguration.Dependencies>();
        services.TryAddTransient<CustomDataSourceAdvanced.Dependencies>();
        services.TryAddTransient<CustomDataSource.Dependencies>();

        services.TryAddTransient<DataSourceCatalog>();
        services.TryAddTransient<IDataSourcesService, DataSourcesService>();
        services.TryAddTransient<DataSourceErrorHelper>();
        services.TryAddTransient(typeof(IDataSourceGenerator<>), typeof(DataSourceGenerator<>));

        services.TryAddTransient<IAppRoot, AppRoot>();

        services.TryAddTransient<QueryFactory>();
        services.TryAddTransient<QueryDefinitionFactory>();
        services.AddTransient<QueryDefinitionService>(); // new v21 SoC split QueryManager
        services.TryAddTransient<QueryManager>();
        services.TryAddTransient(typeof(QueryManager<>)); // new v20
        services.TryAddTransient<ConfigurationDataLoader>();

        services.TryAddTransient<IDataSourceCacheService, DataSourceCacheService>();
        services.TryAddTransient<IListCacheSvc, ListCacheSvc>();

        services.AddDataSourceFallbacks();

        return services;
    }

    public static IServiceCollection AddDataSourceFallbacks(this IServiceCollection services)
    {
        services.TryAddTransient<IAppDataSourcesLoader, AppDataSourcesLoaderUnknown>();

        return services;
    }
}