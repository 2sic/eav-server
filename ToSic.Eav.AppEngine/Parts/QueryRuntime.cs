using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Attributes;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Types.Attributes;

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
                    //if (dataSourceInstance is ICache)
                    //    inStreamNames = null;
                }
                #endregion

                var expectDataAttrib = dataSource.GetCustomAttributes(typeof(ExpectsDataOfType), true)
                    .FirstOrDefault() as ExpectsDataOfType;
                var configType = expectDataAttrib?.StaticName;
                var dsInfo = dataSource.GetCustomAttributes(typeof(DataSourceProperties), true).FirstOrDefault() as DataSourceProperties;
                ICollection<string> inStreamNames = dsInfo?.In;
                result.Add(new DataSourceInfo
                {
                    PartAssemblyAndType = dataSource.FullName + ", " + dataSource.Assembly.GetName().Name,
                    ClassName = dataSource.Name,
                    In = inStreamNames,
                    Out = outStreamNames,
                    ContentType = configType,
                    PrimaryType = dsInfo?.Type.ToString(),
                    Icon = dsInfo?.Icon,
                    DynamicOut = dsInfo?.DynamicOut ?? true
                });
            }

            return result;
        }

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
        }
    }
}
