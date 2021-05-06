using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.DataSources.Catalog
{
    public partial class DataSourceCatalog
    {
        internal string Find(string name)
        {
            // New 11.12.x If the type is identified by a GUID, that's what we should return
            var typeInfo = FindInCache(name);
            if (typeInfo?.Name != null) return typeInfo.Name;

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
            var typeFromCatalog = FindInCache(name)?.Type;
            return typeFromCatalog ?? type;
        }


        /// <summary>
        /// Get all Installed DataSources
        /// </summary>
        /// <remarks>Objects that implement IDataSource</remarks>
        internal IEnumerable<DataSourceInfo> GetAll(bool onlyForVisualQuery)
            => onlyForVisualQuery
                ? Cache.Where(dsi => !string.IsNullOrEmpty(dsi.VisualQuery?.GlobalName))
                : Cache;
    }
}
