using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caches;
using Microsoft.Practices.Unity;

namespace ToSic.Eav.Apps.Parts
{
    public class QueryRuntime: BaseRuntime
    {
        internal QueryRuntime(AppRuntime app) : base(app) { }

        public static IEnumerable<QueryInfoTemp> GetInstalledDataSources()
        {
            var result = new List<QueryInfoTemp>();
            var installedDataSources = DataSource.GetInstalledDataSources();
            foreach (var dataSource in installedDataSources.Where(d => d.GetCustomAttributes(typeof(PipelineDesignerAttribute), false).Any()))
            {
                #region Create Instance of DataSource to get In- and Out-Streams
                ICollection<string> outStreamNames = new string[0];
                ICollection<string> inStreamNames = new string[0];
                if (!dataSource.IsInterface && !dataSource.IsAbstract)
                {
                    var dataSourceInstance = (IDataSource)Activator.CreateInstance(dataSource);
                    try
                    {
                        outStreamNames = dataSourceInstance.Out.Keys;
                    }
                    catch
                    {
                        outStreamNames = null;
                    }
                }
                // Handle Interfaces (currently only ICache) with Unity
                else if (dataSource.IsInterface)
                {
                    var dataSourceInstance = (IDataSource)Factory.Resolve(dataSource);
                    outStreamNames = dataSourceInstance.Out.Keys;
                    if (dataSourceInstance is ICache)
                        inStreamNames = null;
                }
                #endregion

                result.Add(new QueryInfoTemp
                {
                    PartAssemblyAndType = dataSource.FullName + ", " + dataSource.Assembly.GetName().Name,
                    ClassName = dataSource.Name,
                    In = inStreamNames,
                    Out = outStreamNames
                });
            }

            return result;
        }

        public class QueryInfoTemp
        {
            public string PartAssemblyAndType;
            public string ClassName;
            public ICollection<string> In;
            public ICollection<string> Out;
        }
    }
}
