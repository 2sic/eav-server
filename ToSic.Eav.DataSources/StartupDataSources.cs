using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Data.Builder;

namespace ToSic.Eav.DataSources
{
    public static class DataSourcesStartup
    {
        public static IServiceCollection AddDataSources(this IServiceCollection services)
        {
            services.TryAddTransient<IAppRoot, AppRoot>();

            services.TryAddTransient<AttributeBuilder>();
            services.TryAddTransient<Sql>();
            services.TryAddTransient<DataTable>();

            return services;
        }
    }
}