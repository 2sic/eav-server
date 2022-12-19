using System;
using System.Collections.Generic;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DataSources.Catalog
{
    [PrivateApi]
    public partial class DataSourceCatalog: HasLog
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
            var wrapLog = Log.Fn<ICollection<string>>();

            try
            {
                // This MUST use Build (not GetService<>) since that will also create objects which are not registered
                var dataSourceInstance = ServiceProvider.Build<IDataSource>(dsInfo.Type);

                // skip this if out-connections cannot be queried
                return wrapLog.Return(dataSourceInstance.Out.Keys);
            }
            catch
            {
                return wrapLog.ReturnNull("error");
            }
        }
    }
}
