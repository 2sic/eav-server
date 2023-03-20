using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.DataSources.Catalog
{
    public partial class DataSourceCatalog
    {
        /// <summary>
        /// A cache of all DataSource Types - initialized upon first access ever, then static cache.
        /// </summary>
        public static IDictionary<int, List<DataSourceInfo>> AppCache { get; } =
            new ConcurrentDictionary<int, List<DataSourceInfo>>();

        public void UpdateAppCache(int appId, IEnumerable<Type> appDataSources)
        {
            AppCache[appId] = (appDataSources ?? new List<Type>())
                .Select(t => new DataSourceInfo(t, false))
                .ToList();
        }

    }
}
