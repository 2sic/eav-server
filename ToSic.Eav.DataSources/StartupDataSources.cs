using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.DataSources.Internal;
using ToSic.Eav.Services;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupDataSources
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IServiceCollection AddDataSources(this IServiceCollection services)
    {
        // Dependencies, new in v15
        //services.TryAddTransient<DataSourceBase.MyServices>();
        services.TryAddTransient<App.MyServices>();
        
        //services.TryAddTransient<IDataSourceConfiguration, DataSourceConfiguration>();
        //services.TryAddTransient<DataSourceConfiguration.MyServices>();
        //services.TryAddTransient<CustomDataSourceAdvanced.MyServices>();
        //services.TryAddTransient<CustomDataSource.MyServices>();

        //services.TryAddTransient<DataSourceCatalog>();
        services.TryAddTransient<IDataSourcesService, DataSourcesService>();
        //services.TryAddTransient<DataSourceErrorHelper>();
        services.TryAddTransient(typeof(IDataSourceGenerator<>), typeof(DataSourceGenerator<>));

        //services.TryAddTransient<IAppRoot, AppRoot>();

        services.TryAddTransient<Sql>();
        services.TryAddTransient<Sql.MyServices>();
        services.TryAddTransient<SqlPlatformInfo>();

        services.TryAddTransient<DataTable>();

        //services.TryAddTransient<QueryBuilder>();
        //services.TryAddTransient<QueryDefinitionBuilder>();
        //services.TryAddTransient<QueryManager>();

        services.TryAddTransient<ValueLanguages>();

        services.TryAddTransient<ITreeMapper, TreeMapper>();

        //services.TryAddTransient<ConfigurationDataLoader>();

        //services.TryAddTransient<IDataSourceCacheService, DataSourceCacheService>();
        //services.TryAddTransient<IListCacheSvc, ListCacheSvc>();

        //services.AddDataSourcesFallback();

        return services;
    }

    //[ShowApiWhenReleased(ShowApiMode.Never)]
    //public static IServiceCollection AddDataSourcesFallback(this IServiceCollection services)
    //{
    //    //services.TryAddTransient<IAppDataSourcesLoader, AppDataSourcesLoaderUnknown>();

    //    return services;
    //}
}