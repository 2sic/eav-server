﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.Metadata.Targets;
using ToSic.Eav.Persistence.Efc.Sys.DbContext;
using ToSic.Eav.Persistence.Efc.Sys.Services;
using ToSic.Eav.Repositories.Sys;
using ToSic.Eav.Repository.Efc.Sys;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;
using ToSic.Sys.Boot;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Startup;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupRepositoryEfc
{
    public static IServiceCollection AddRepositoryAndEfc(this IServiceCollection services)
    {
        services.TryAddTransient<DbStorage>();
        services.TryAddTransient<ITargetTypesLoader, EfcMetadataTargetTypesLoaderService>();

        // transient lifetime is important, otherwise 2-3x slower!
        // note: https://docs.microsoft.com/en-us/ef/core/miscellaneous/configuring-dbcontext says we should use transient
#if NETFRAMEWORK
            //services.AddDbContext<EavDbContext>(contextLifetime: ServiceLifetime.Transient);
            services.AddDbContext<EavDbContext>(contextLifetime: ServiceLifetime.Transient, optionsLifetime: ServiceLifetime.Singleton);
#else
        // DbContextOptions optionsLifetime is Singleton (in efcore 3.1+, aka oqtane) to fix issue
        // "Cannot resolve 'ToSic.Eav.Repositories.IRepositoryLoader' from root provider because it
        // requires scoped service 'Microsoft.EntityFrameworkCore.DbContextOptions`1[ToSic.Eav.Persistence.Efc.Models.EavDbContext]'"
        // Transient, or Scoped for optionsLifetime is not working.
        services.AddDbContext<EavDbContext>(contextLifetime: ServiceLifetime.Transient, optionsLifetime: ServiceLifetime.Singleton);
#endif

        // Inner loader for use of the main RepositoryLoader
        services.TryAddTransient<EfcAppLoaderService>();
        services.TryAddTransient<IAppsAndZonesLoader, EfcAppsAndZonesLoader>();
        services.TryAddTransient<IAppsAndZonesLoaderWithRaw, EfcAppsAndZonesLoader>();
        services.AddTransient<IBootProcess, BootWarmUpEavSql>();

        services.TryAddTransient<IStorageFactory, StorageFactoryEfc>();

        return services;
    }
}