using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.DataSources.Catalog
{
    public partial class DataSourceCatalog
    {
        /// <summary>
        /// A cache of all DataSource Types - initialized upon first access ever, then static cache.
        /// </summary>
        private static List<DataSourceInfo> GlobalCache { get; } = AssemblyHandling
            .FindInherited(typeof(IDataSource))
            .Select(t => new DataSourceInfo(t))
            .ToList();

        /// <summary>
        /// Find a DataSource which may have changed it's name. Will look in the cached names list.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static DataSourceInfo FindInCache(string name, int appId)
        {
            var list = GlobalCache;
            var inGlobal = FindInCachedList(name, list);
            if (inGlobal != null) return inGlobal;

            // New v15.04 - also support app level data sources
            return !AppCache.TryGetValue(appId, out var appCache) 
                ? null 
                : FindInCachedList(name, appCache);
        }

        private static DataSourceInfo FindInCachedList(string name, List<DataSourceInfo> list) =>
            // First check for normal type name
            list.FirstOrDefault(dst => dst.Name.EqualsInsensitive(name))
            // Otherwise check for historical names in the VisualQuery Attribute
            ?? list.FirstOrDefault(dst =>
                dst.VisualQuery?.PreviousNames.Any(pn => pn.EqualsInsensitive(name)) ?? false);
    }
}
