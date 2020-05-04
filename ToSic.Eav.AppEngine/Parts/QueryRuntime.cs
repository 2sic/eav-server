using System;
using System.Collections.Generic;
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
        public static IEnumerable<DataSourceInfo> GetInstalledDataSources()
        {
            var result = new List<DataSourceInfo>();
            var installedDataSources = Catalog.GetAll(true);
            foreach (var dataSource in installedDataSources)
            {
                #region Create Instance of DataSource to get In- and Out-Streams
                ICollection<string> outStreamNames = new string[0];
                if (!dataSource.Type.IsInterface && !dataSource.Type.IsAbstract)
                {
                    var dataSourceInstance = (IDataSource)Activator.CreateInstance(dataSource.Type);
                    if (dataSourceInstance.OutIsDynamic) // skip this if out-connections cannot be queried
                        outStreamNames = null;
                    else
                        try
                        {
                            outStreamNames = dataSourceInstance.Out.Keys;
                        }
                        catch
                        {
                            outStreamNames = null;
                        }
                }
                // Handle Interfaces (currently only on ICache)
                else if (dataSource.Type.IsInterface)
                {
                    var dataSourceInstance = (IDataSource)Factory.Resolve(dataSource.Type);
                    outStreamNames = dataSourceInstance.Out.Keys;
                }
                #endregion

                //var dsInfo = dataSource.VisualQuery;//.GetCustomAttributes(typeof(VisualQueryAttribute), true).FirstOrDefault() as VisualQueryAttribute;
                result.Add(new DataSourceInfo(dataSource.Type.Name, dataSource.VisualQuery)
                {
                    PartAssemblyAndType = dataSource.GlobalName,// dataSource.Type.FullName + ", " + dataSource.Type.Assembly.GetName().Name,
                    Out = outStreamNames
                });
            }

            return result;
        }

        /// <summary>
        /// Get a query definition from the current app
        /// </summary>
        /// <param name="queryId"></param>
        /// <returns></returns>
        public QueryDefinition Get(int queryId) =>
            new QueryDefinition(Eav.DataSources.Queries.QueryManager.GetQueryEntity(queryId, AppRT./*Cache*/AppState), AppRT.AppId, Log);

        public class DataSourceInfo
        {
            public string PartAssemblyAndType;
            //public string ClassName;
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
