using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Parts
{
    public class QueryRuntime: RuntimeBase
    {
        internal QueryRuntime(AppRuntime appRt, ILog parentLog) : base(appRt, parentLog) { }

        /// <summary>
        /// Get all installed data sources - usually for the UI
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<DataSourceInfo> QueryDataSources()
        {
            var installedDataSources = Catalog.GetAll(true);

            return installedDataSources.Select(dataSource => new DataSourceInfo(dataSource.Type.Name, dataSource.VisualQuery)
            {
                PartAssemblyAndType = dataSource.GlobalName, 
                Out = GetOutStreamNames(dataSource)
            }).ToList();
        }

        /// <summary>
        /// Create Instance of DataSource to get In- and Out-Streams
        /// </summary>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        private static ICollection<string> GetOutStreamNames(Eav.DataSources.DataSourceInfo dataSource)
        {
            if (dataSource.Type.IsAbstract) return null;

            // Handle Interfaces and real types (currently only on ICache)
            var dataSourceInstance = (IDataSource) Factory.Resolve(dataSource.Type);
            
            try
            {
                // skip this if out-connections cannot be queried
                return dataSourceInstance.OutIsDynamic ? null : dataSourceInstance.Out.Keys;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get a query definition from the current app
        /// </summary>
        /// <param name="queryId"></param>
        /// <returns></returns>
        public QueryDefinition Get(int queryId) =>
            new QueryDefinition(Eav.DataSources.Queries.QueryManager.GetQueryEntity(queryId, AppRT.AppState), AppRT.AppId, Log);

        public class DataSourceInfo
        {
            public string PartAssemblyAndType;
            public ICollection<string> In;
            public ICollection<string> Out;
            public string ContentType;
            public string PrimaryType;
            public string Icon;
            public bool DynamicOut;
            public string HelpLink;
            public bool EnableConfig;
            public string Name;
            public string UiHint;
            public int Difficulty;

            public DataSourceInfo(string fallbackName, VisualQueryAttribute dsInfo)
            {
                Name = fallbackName; // will override further down if dsInfo is provided
                if (dsInfo == null) return;
                UiHint = dsInfo.UiHint;
                PrimaryType = dsInfo.Type.ToString();
                Icon = dsInfo.Icon;
                HelpLink = dsInfo.HelpLink;
                In = dsInfo.In;
                DynamicOut = dsInfo.DynamicOut ;
                EnableConfig = dsInfo.EnableConfig;
                ContentType = dsInfo.ExpectsDataOfType;
                if (!string.IsNullOrEmpty(dsInfo.NiceName))
                    Name = dsInfo.NiceName;
                Difficulty = (int) dsInfo.Difficulty;
            }
        }
    }
}
