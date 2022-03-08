using System;
using System.Collections.Generic;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.DataSources.Catalog
{
    [PrivateApi]
    public partial class DataSourceCatalog: HasLog<DataSourceCatalog>
    {
        public DataSourceCatalog(IServiceProvider serviceProvider) : base("DS.DsCat")
        {
            ServiceProvider = serviceProvider;
        }
        private IServiceProvider ServiceProvider { get; }

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
                // This MUST use Build (not GetService<>) since that will also create objects which are not registered
                var dataSourceInstance = ServiceProvider.Build<IDataSource>(dsInfo.Type);

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
