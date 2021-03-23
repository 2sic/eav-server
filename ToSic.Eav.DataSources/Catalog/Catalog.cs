using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.DataSources.Catalog
{
    internal class CatalogHelpers
    {
        internal static string FindName(string name)
        {
            // New 11.12.x If the type is identified by a GUID, that's what we should return
            var typeInfo = FindInDsTypeCache(name);
            if (typeInfo?.GlobalName != null) return typeInfo.GlobalName;

            // Old mechanism which checks real types etc but probably is never needed any more
            var type = FindType(name);
            if (type == null) return name;
            var longName = type.AssemblyQualifiedName;
            var first = longName.IndexOf(',');
            var second = longName.IndexOf(',', first + 2);
            var newName = longName.Substring(0, second);
            return newName;
        }

        internal static Type FindType(string name)
        {
            // first try to just find the type, but check if it's marked [Obsolete]
            var type = Type.GetType(name);
            if (type != null && !type.GetCustomAttributes(typeof(ObsoleteAttribute), inherit: false).Any())
                return type;

            // if not found, or if obsolete, try to find another
            var typeFromCatalog = FindInDsTypeCache(name)?.Type;
            return typeFromCatalog ?? type;
        }

        /// <summary>
        /// Find a DataSource which may have changed it's name. Will look in the cached names list.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static DataSourceInfo FindInDsTypeCache(string name) =>
            DsTypeCache.FirstOrDefault(dst =>
                string.Equals(dst.GlobalName, name,
                    StringComparison.InvariantCultureIgnoreCase))
            ?? DsTypeCache.FirstOrDefault(dst =>
                dst.VisualQuery?.PreviousNames.Any(pn => string.Equals(pn, name,
                    StringComparison.InvariantCultureIgnoreCase)) ?? false);

        /// <summary>
        /// Get all Installed DataSources
        /// </summary>
        /// <remarks>Objects that implement IDataSource</remarks>
        internal static IEnumerable<DataSourceInfo> GetAll(bool onlyForVisualQuery)
            => onlyForVisualQuery
                ? DsTypeCache.Where(dsi => !string.IsNullOrEmpty(dsi.VisualQuery?.GlobalName))
                : DsTypeCache;

        /// <summary>
        /// A cache of all DataSource Types - initialized upon first access ever, then static cache.
        /// </summary>
        private static List<DataSourceInfo> DsTypeCache { get; } = Plumbing.AssemblyHandling
            .FindInherited(typeof(IDataSource))
            .Select(t => new DataSourceInfo(t)).ToList();
    }
}
