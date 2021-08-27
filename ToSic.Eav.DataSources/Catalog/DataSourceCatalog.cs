using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.DataSources.Catalog
{
    [PrivateApi]
    public partial class DataSourceCatalog: HasLog<DataSourceCatalog>
    {
        public IServiceProvider ServiceProvider { get; }
        public DataSourceCatalog(IServiceProvider serviceProvider) : base("DS.DsCat")
        {
            ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Create Instance of DataSource to get In- and Out-Streams
        /// </summary>
        /// <param name="dsInfo"></param>
        /// <returns></returns>
        public ICollection<string> GetOutStreamNames(DataSourceInfo dsInfo)
        {
            var wrapLog = Log.Call<ICollection<string>>();
            // 2021-03-23 2dm - disabled this, as it prevented interfaces from instantiating
            // Since DI will find the correct DataSource it should work even with abstract classes, since they should be implemented
            //if (dataSource.Type.IsAbstract) return null;

            try
            {
                // Handle Interfaces and real types (currently only on ICache / IAppRoot)
                // TODO: STV - this will probably fail in Oqtane, because the types are not registered
                // To make this work, we probably need to scan all DLLs for IDataSources and register them in DI
                // Pls check
                var dataSourceInstance = (IDataSource)ServiceProvider.GetRequiredService(dsInfo.Type);// (IDataSource)Factory.Resolve(dsInfo.Type);

                // skip this if out-connections cannot be queried
                return dataSourceInstance.Out.Keys;
            }
            catch
            {
                return wrapLog("error", null);
            }
        }
    }
}
