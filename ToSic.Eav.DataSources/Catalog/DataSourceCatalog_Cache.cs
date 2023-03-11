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
        private static List<DataSourceInfo> Cache { get; } = AssemblyHandling
            .FindInherited(typeof(IDataSource))
            .Select(t => new DataSourceInfo(t))
            .ToList();

        /// <summary>
        /// Find a DataSource which may have changed it's name. Will look in the cached names list.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static DataSourceInfo FindInCache(string name) =>
            Cache.FirstOrDefault(dst => dst.Name.EqualsInsensitive(name))
            ?? Cache.FirstOrDefault(dst => dst.VisualQuery?.PreviousNames.Any(pn => pn.EqualsInsensitive(name)) ?? false);

    }
}
