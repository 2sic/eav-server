using Microsoft.EntityFrameworkCore;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.DataSources.RootSources;
using ToSic.Eav.DataSources.SqlSources;
using ToSic.Eav.ImportExport.Interfaces;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence.Efc;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Repository.Efc.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
            serviceCollection.TryAddTransient<ICache, QuickCache>();
            serviceCollection.TryAddTransient<IRootSource, EavSqlStore>();

            serviceCollection.TryAddTransient<IRepositoryImporter, RepositoryImporter>();

            serviceCollection.TryAddTransient<ISystemConfiguration, Configuration>();

            serviceCollection.TryAddTransient<IRepositoryLoader, Efc11Loader>();

            var conStr = new Configuration().DbConnectionString;
            if (!conStr.ToLower().Contains("multipleactiveresultsets")) // this is needed to allow querying data while preparing new data on the same DbContext
                conStr += ";MultipleActiveResultSets=True";

            serviceCollection.AddDbContext<EavDbContext>(options => options.UseSqlServer(conStr), 
                ServiceLifetime.Transient); // transient lifetime is important, otherwise 2-3x slower!

            // register some Default Constructors
            serviceCollection.TryAddTransient<SqlDataSource>();
            serviceCollection.TryAddTransient<DataTableDataSource>();

            
        }
        #endregion

    }
}
