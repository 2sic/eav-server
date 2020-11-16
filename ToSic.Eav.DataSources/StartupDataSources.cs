using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.DataSources.Queries;

namespace ToSic.Eav.DataSources
{
    public static class DataSourcesStartup
    {
        public static IServiceCollection AddDataSources(this IServiceCollection services)
        {
            services.TryAddTransient<DataSourceFactory>();

            services.TryAddTransient<IAppRoot, AppRoot>();

            services.TryAddTransient<AttributeBuilder>();
            services.TryAddTransient<Sql>();
            services.TryAddTransient<DataTable>();

            services.TryAddTransient<GlobalQueries>();
            services.TryAddTransient<QueryBuilder>();

            return services;
        }
    }
}