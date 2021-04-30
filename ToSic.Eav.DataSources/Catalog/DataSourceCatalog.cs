using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.DataSources.Catalog
{
    [PrivateApi]
    public partial class DataSourceCatalog: HasLog
    {
        public DataSourceCatalog(ILog parentLog = null) : base("DS.DsCat", parentLog)
        {
        }

        /// <summary>
        /// Get all installed data sources - usually for the UI
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DataSourceDto> QueryDataSources()
        {
            var callLog = Log.Call<IEnumerable<DataSourceDto>>();
            var installedDataSources = GetAll(true);

            var result = installedDataSources
                .Select(dsInfo => new DataSourceDto(dsInfo.Type.Name, dsInfo.VisualQuery)
                {
                    PartAssemblyAndType = dsInfo.Name,
                    Out = dsInfo.VisualQuery?.DynamicOut == true ? null : GetOutStreamNames(dsInfo)
                })
                .ToList();

            return callLog(result.Count.ToString(), result);
        }

        /// <summary>
        /// Create Instance of DataSource to get In- and Out-Streams
        /// </summary>
        /// <param name="dsInfo"></param>
        /// <returns></returns>
        private ICollection<string> GetOutStreamNames(DataSourceInfo dsInfo)
        {
            var wrapLog = Log.Call<ICollection<string>>();
            // 2021-03-23 2dm - disabled this, as it prevented interfaces from instantiating
            // Since DI will find the correct DataSource it should work even with abstract classes, since they should be implemented
            //if (dataSource.Type.IsAbstract) return null;

            try
            {
                // Handle Interfaces and real types (currently only on ICache / IAppRoot)
                var dataSourceInstance = (IDataSource)Factory.Resolve(dsInfo.Type);

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
