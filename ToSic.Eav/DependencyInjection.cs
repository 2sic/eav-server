using Microsoft.EntityFrameworkCore;
using ToSic.Eav.DataSources;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence.Efc;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Repository.Efc.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Caching;
using ToSic.Eav.Metadata;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.ImportExport.Persistence.File;
using ToSic.Eav.Repositories;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Basic;
using ToSic.Eav.Serialization;

namespace ToSic.Eav
{
	/// <summary>
	/// Global Eav Configuration
	/// </summary>
	public static class DependencyInjection
	{
	    /// <summary>
	    /// Use this to setup the new DI container
	    /// </summary>
	    /// <param name="services"></param>
	    public static IServiceCollection AddEav(this IServiceCollection services)
	    {
            // very basic stuff - normally overriden by the platform
            services.TryAddTransient<IFingerprint, BasicFingerprint>();
            services.TryAddTransient<IUser, BasicUser>();
            services.TryAddTransient<IValueConverter, BasicValueConverter>();

            // core things - usually not replaced
            services.TryAddTransient<IRuntime, Runtime>();

            services.TryAddSingleton<IAppsCache, AppsCache>();

            services.TryAddTransient<IAppRoot, AppRoot>();
	        services.TryAddTransient<IRemoteMetadata, RemoteMetadata>();
	        services.TryAddTransient<ITargetTypes, EfcMetadataTargetTypes>();

            services.TryAddTransient<IRepositoryImporter, RepositoryImporter>();

            services.TryAddTransient<ISystemConfiguration, Repository.Efc.Implementations.Configuration>();

            services.TryAddTransient<IRepositoryLoader, Efc11Loader>();

	        services.TryAddTransient<IDataSerializer, JsonSerializer>();
	        services.TryAddTransient<IDataDeserializer, JsonSerializer>();
            services.TryAddTransient<ZipExport, ZipExport>();
            services.TryAddTransient<ZipImport, ZipImport>();
            services.TryAddTransient<ZipFromUrlImport, ZipFromUrlImport>();

            var conStr = new Repository.Efc.Implementations.Configuration().DbConnectionString;
            if (!conStr.ToLower().Contains("multipleactiveresultsets")) // this is needed to allow querying data while preparing new data on the same DbContext
                conStr += ";MultipleActiveResultSets=True";

            // transient lifetime is important, otherwise 2-3x slower!
            // note: https://docs.microsoft.com/en-us/ef/core/miscellaneous/configuring-dbcontext says we should use transient
            services.AddDbContext<EavDbContext>(options => options.UseSqlServer(conStr), 
                ServiceLifetime.Transient); 

            // register some Default Constructors
            services.TryAddTransient<Sql>();
            services.TryAddTransient<DataTable>();

            return services;
        }
    }
}
