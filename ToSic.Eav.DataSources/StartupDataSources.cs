using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.DataSources.Queries;

namespace ToSic.Eav.DataSources
{
    public static class DataSourcesStartup
    {
        public static IServiceCollection AddDataSources(this IServiceCollection services)
        {
            services.TryAddTransient<DataSourceFactory>();

            services.TryAddTransient<IAppRoot, AppRoot>();

            services.TryAddTransient<Sql>();
            services.TryAddTransient<DataTable>();

            services.TryAddTransient<GlobalQueries>();
            services.TryAddTransient<QueryBuilder>();

            services.TryAddTransient<ValueLanguages>();

            return services;
        }
    }
}