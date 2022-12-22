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
            services.TryAddTransient<DataSource.Dependencies>();
            services.TryAddTransient<App.Dependencies>();
            services.TryAddTransient<DataSourceConfiguration.Dependencies>();

            services.TryAddTransient<DataSourceCatalog>();
            services.TryAddTransient<DataSourceFactory>();
            services.TryAddTransient<DataSourceErrorHandling>();

            services.TryAddTransient<IAppRoot, AppRoot>();

            services.TryAddTransient<Sql>();
            services.TryAddTransient<Sql.Dependencies>();
            services.TryAddTransient<SqlPlatformInfo, SqlPlatformInfo>();

            services.TryAddTransient<DataTable>();

            services.TryAddTransient<QueryBuilder>();

            services.TryAddTransient<ValueLanguages>();

            return services;
        }
    }
}