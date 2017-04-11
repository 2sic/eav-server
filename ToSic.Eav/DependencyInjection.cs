using Microsoft.Practices.Unity;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.DataSources.RootSources;
using ToSic.Eav.DataSources.SqlSources;
using ToSic.Eav.Implementations;
using ToSic.Eav.ImportExport.Interfaces;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav
{
	/// <summary>
	/// Global Eav Configuration
	/// </summary>
	public class DependencyInjection
	{
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

            // register some Default Constructors
            cont.RegisterType<SqlDataSource>(new InjectionConstructor());
            cont.RegisterType<DataTableDataSource>(new InjectionConstructor());

            
            return cont;
        }
        #endregion
    }
}
