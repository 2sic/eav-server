using Microsoft.EntityFrameworkCore;
using Microsoft.Practices.Unity;
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

//using ToSic.Eav.Repository.EF4;

namespace ToSic.Eav
{
	/// <summary>
	/// Global Eav Configuration
	/// </summary>
	public class DependencyInjection
	{


        #region try to configure the ef-core container

     //   public static ServiceCollection ServiceCollection 
     //       => _serviceCollection ?? (_serviceCollection = new ServiceCollection());

	    //public static IServiceProvider ServiceProvider
	    //    => (_serviceCollection ?? (_serviceCollection = new ServiceCollection()))
	    //        .BuildServiceProvider();

	    //private static IServiceProvider _serviceprovider;
	    //private static ServiceCollection _serviceCollection;

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



        #region Configure Unity Factory with defaults

        /// <summary>
        /// Register Types in Unity Container
        /// </summary>
        /// <remarks>If Unity is not configured in App/Web.config this can be used</remarks>
        public IUnityContainer ConfigureDefaultMappings(IUnityContainer cont)
        {
            if (!cont.IsRegistered<ICache>())
                cont.RegisterType<ICache, QuickCache>();
            if (!cont.IsRegistered<IRootSource>())
                cont.RegisterType<IRootSource, EavSqlStore>();

            if (!cont.IsRegistered<IRepositoryImporter>())
                cont.RegisterType<IRepositoryImporter, RepositoryImporter>();

            if (!cont.IsRegistered<ISystemConfiguration>())
                cont.RegisterType<ISystemConfiguration, Configuration>();

            if (!cont.IsRegistered<IRepositoryLoader>())
                cont.RegisterType<IRepositoryLoader, Efc11Loader>(
                    new InjectionConstructor(new ResolvedParameter<EavDbContext>()));


            var conStr = new Configuration().DbConnectionString;
            if (!conStr.ToLower().Contains("multipleactiveresultsets")) // this is needed to allow querying data while preparing new data on the same DbContext
                conStr += ";MultipleActiveResultSets=True";

            #region register the new EF1.1 ORM
            if (!cont.IsRegistered<EavDbContext>())
                cont.RegisterType<EavDbContext>(new InjectionFactory(
                    obj =>
                    {
                        var opts = new DbContextOptionsBuilder<EavDbContext>();
                        opts.UseSqlServer(conStr);
                        return new EavDbContext(opts.Options);
                    }));
            #endregion

            // register some Default Constructors
            cont.RegisterType<SqlDataSource>(new InjectionConstructor());
            cont.RegisterType<DataTableDataSource>(new InjectionConstructor());

            
            return cont;
        }

        #endregion
    }
}
