using Microsoft.EntityFrameworkCore;
using ToSic.Eav.DataSources;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence.Efc;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Repository.Efc.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Caching;
using ToSic.Eav.DataSources.Caching;
using ToSic.Eav.Metadata;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Repositories;
using ToSic.Eav.Run;

namespace ToSic.Eav
{
	/// <summary>
	/// Global Eav Configuration
	/// </summary>
	public class DependencyInjection
	{
        #region try to configure the ef-core container

	    /// <summary>
	    /// Use this to setup the new DI container
	    /// </summary>
	    /// <param name="serviceCollection"></param>
	    public void ConfigureNetCoreContainer(IServiceCollection serviceCollection)
	    {
            // 2019-12-11 2dm new
            serviceCollection.TryAddSingleton<IAppsCache, AppsCache>();

            serviceCollection.TryAddTransient<IAppRoot, AppRoot>();
            //serviceCollection.TryAddTransient<IAppsLoader, EavSqlStore>();
	        serviceCollection.TryAddTransient<IRemoteMetadata, RemoteMetadata>();
	        serviceCollection.TryAddTransient<ITargetTypes, EfcMetadataTargetTypes>();

            serviceCollection.TryAddTransient<IRepositoryImporter, RepositoryImporter>();

            serviceCollection.TryAddTransient<ISystemConfiguration, Repository.Efc.Implementations.Configuration>();

            serviceCollection.TryAddTransient<IRepositoryLoader, Efc11Loader>();

	        serviceCollection.TryAddTransient<IThingSerializer, JsonSerializer>();
	        serviceCollection.TryAddTransient<IThingDeserializer, JsonSerializer>();

            var conStr = new Repository.Efc.Implementations.Configuration().DbConnectionString;
            if (!conStr.ToLower().Contains("multipleactiveresultsets")) // this is needed to allow querying data while preparing new data on the same DbContext
                conStr += ";MultipleActiveResultSets=True";

            serviceCollection.AddDbContext<EavDbContext>(options => options.UseSqlServer(conStr), 
                ServiceLifetime.Transient); // transient lifetime is important, otherwise 2-3x slower!

            // register some Default Constructors
            serviceCollection.TryAddTransient<Sql>();
            serviceCollection.TryAddTransient<DataTable>();

            
        }
        #endregion

    }
}
