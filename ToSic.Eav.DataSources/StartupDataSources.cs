using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Query;
using ToSic.Eav.DataSources.Catalog;
using ToSic.Eav.Services;

namespace ToSic.Eav.DataSources
{
    public static class DataSourcesStartup
    {
        public static IServiceCollection AddDataSources(this IServiceCollection services)
        {
            // Dependencies, new in v15
            services.TryAddTransient<DataSource.MyServices>();
            services.TryAddTransient<CustomDataSourceAdvanced.MyServices>();
            services.TryAddTransient<App.MyServices>();
            services.TryAddTransient<DataSourceConfiguration>();
            services.TryAddTransient<DataSourceConfiguration.MyServices>();

            services.TryAddTransient<DataSourceCatalog>();
            services.TryAddTransient<IDataSourcesService, DataSourcesService>();
            services.TryAddTransient<DataSourceErrorHelper>();
            services.TryAddTransient(typeof(IDataSourceGenerator<>), typeof(DataSourceGenerator<>));

            services.TryAddTransient<IAppRoot, AppRoot>();

            services.TryAddTransient<Sql>();
            services.TryAddTransient<Sql.MyServices>();
            services.TryAddTransient<SqlPlatformInfo, SqlPlatformInfo>();

            services.TryAddTransient<DataTable>();

            services.TryAddTransient<QueryBuilder>();
            services.TryAddTransient<QueryDefinitionBuilder>();

            services.TryAddTransient<ValueLanguages>();

            services.TryAddTransient<ITreeMapper, TreeMapper>();

            services.TryAddTransient<ConfigurationDataLoader>();

            services.AddDataSourcesFallback();

            return services;
        }

        public static IServiceCollection AddDataSourcesFallback(this IServiceCollection services)
        {
            services.TryAddTransient<IAppDataSourcesLoader, AppDataSourcesLoaderUnknown>();

            return services;
        }
    }
}