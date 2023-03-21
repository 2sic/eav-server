using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.DataSources.Catalog;
using ToSic.Eav.DataSources.Queries;

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
            services.TryAddTransient<DataSourceConfigurationManager>();
            services.TryAddTransient<DataSourceConfigurationManager.MyServices>();

            services.TryAddTransient<DataSourceCatalog>();
            services.TryAddTransient<IDataSourceFactory, DataSourceFactory>();
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

            

            return services;
        }
    }
}