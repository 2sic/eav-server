using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Metadata;
using ToSic.Eav.Persistence.Efc;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Repository.Efc
{
    public static class RepositoryStartup
    {
        public static IServiceCollection AddRepositoryAndEfc(this IServiceCollection services, string connectionString)
        {
            services.TryAddTransient<ITargetTypes, EfcMetadataTargetTypes>();

            //var conStr = new Repository.Efc.Implementations.Configuration().DbConnectionString;
            if (!connectionString.ToLower().Contains("multipleactiveresultsets")) // this is needed to allow querying data while preparing new data on the same DbContext
                connectionString += ";MultipleActiveResultSets=True";

            // transient lifetime is important, otherwise 2-3x slower!
            // note: https://docs.microsoft.com/en-us/ef/core/miscellaneous/configuring-dbcontext says we should use transient
            services.AddDbContext<EavDbContext>(options => options.UseSqlServer(connectionString),
                ServiceLifetime.Transient);

            services.TryAddTransient<IRepositoryLoader, Efc11Loader>();

            return services;
        }
    }
}
