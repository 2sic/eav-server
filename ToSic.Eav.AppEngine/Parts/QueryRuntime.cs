using System;
using System.Collections.Generic;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caches;

namespace ToSic.Eav.Apps.Parts
{
    public class QueryRuntime: RuntimeBase
    {
        internal QueryRuntime(AppRuntime app) : base(app) { }

        public static IEnumerable<QueryInfoTemp> GetInstalledDataSources()
        {
            var result = new List<QueryInfoTemp>();
            var installedDataSources = DataSource.GetInstalledDataSources(true);
            foreach (var dataSource in installedDataSources)
            {
                #region Create Instance of DataSource to get In- and Out-Streams
                ICollection<string> outStreamNames = new string[0];
                ICollection<string> inStreamNames = new string[0];
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
