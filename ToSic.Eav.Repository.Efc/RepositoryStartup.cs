using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Metadata;
using ToSic.Eav.Persistence.Efc;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Repository.Efc
{
    public static class StartupRepositoryEfc
    {
        public static IServiceCollection AddRepositoryAndEfc(this IServiceCollection services)
        {
            services.TryAddTransient<ITargetTypes, EfcMetadataTargetTypes>();

            // transient lifetime is important, otherwise 2-3x slower!
            // note: https://docs.microsoft.com/en-us/ef/core/miscellaneous/configuring-dbcontext says we should use transient
            services.AddDbContext<EavDbContext>(ServiceLifetime.Transient);

            services.TryAddTransient<IRepositoryLoader, Efc11Loader>();

            services.TryAddTransient<DbDataController>();

            return services;
        }
    }
}
