using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Attributes;
using ToSic.Eav.DataSources.Pipeline;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps.Parts
{
    public class QueryRuntime: RuntimeBase
    {
        internal QueryRuntime(AppRuntime app, Log parentLog) : base(app, parentLog) { }

        public static IEnumerable<DataSourceInfo> GetInstalledDataSources()
        {
            var result = new List<DataSourceInfo>();
            var installedDataSources = DataSource.GetInstalledDataSources(true);
            foreach (var dataSource in installedDataSources)
            {
                #region Create Instance of DataSource to get In- and Out-Streams
                ICollection<string> outStreamNames = new string[0];
                if (!dataSource.IsInterface && !dataSource.IsAbstract)
                {
                    var dataSourceInstance = (IDataSource)Activator.CreateInstance(dataSource);
                    if (dataSourceInstance.TempUsesDynamicOut) // skip this if out-connections cannot be queried
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
                else if (dataSource.IsInterface)
                {
                    var dataSourceInstance = (IDataSource)Factory.Resolve(dataSource);
                    outStreamNames = dataSourceInstance.Out.Keys;
                }
                #endregion

                var dsInfo = dataSource.GetCustomAttributes(typeof(DataSourceProperties), true).FirstOrDefault() as DataSourceProperties;
                result.Add(new DataSourceInfo(dataSource.Name, dsInfo)
                {
                    PartAssemblyAndType = dataSource.FullName + ", " + dataSource.Assembly.GetName().Name,
                    Out = outStreamNames
                });
            }

            return result;
        }

        public QueryDefinition Get(int queryId) =>
            new QueryDefinition(DataPipeline.GetPipelineEntity(queryId, App.Cache));

        public class DataSourceInfo
        {
            public string PartAssemblyAndType;
            public string ClassName;
            public ICollection<string> In;
            public ICollection<string> Out;
            public string ContentType;
            public string PrimaryType;
            public string Icon;
            public bool DynamicOut;
            public string HelpLink;
            public bool EnableConfig;
            public string Name;

            public DataSourceInfo(string fallbackName, DataSourceProperties dsInfo)
            {
                Name = fallbackName; // will override afterwards if possible
                if (dsInfo == null) return;
                PrimaryType = dsInfo.Type.ToString();
                Icon = dsInfo.Icon;
                HelpLink = dsInfo.HelpLink;
                In = dsInfo.In;
                DynamicOut = dsInfo.DynamicOut ;
                EnableConfig = dsInfo.EnableConfig;
                ContentType = dsInfo.ExpectsDataOfType;
                if (!string.IsNullOrEmpty(dsInfo.NiceName))
                    Name = dsInfo.NiceName;
            }
        }
    }
}
