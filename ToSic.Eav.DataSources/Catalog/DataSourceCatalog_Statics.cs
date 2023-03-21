using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.DataSources.Catalog
{
    public partial class DataSourceCatalog
    {
        internal string Find(string name, int appId)
        {
            // New 11.12.x If the type is identified by a GUID, that's what we should return
            var typeInfo = FindInCache(name, appId);
            if (typeInfo?.Name != null) return typeInfo.Name;

            // Old mechanism which checks real types etc but probably is never needed any more
            var type = FindType(name, appId);
            if (type == null) return name;
            var longName = type.AssemblyQualifiedName;
            var first = longName.IndexOf(',');
            var second = longName.IndexOf(',', first + 2);
            var newName = longName.Substring(0, second);
            return newName;
        }
        internal Type FindTypeByGuidOrName(string name, int appId)
        {
            // New 11.12.x If the type is identified by a GUID, that's what we should return
            var typeInfo = FindInCache(name, appId);
            if (typeInfo?.Name != null) return typeInfo.Type;

            // Old mechanism which checks real types etc but probably is never needed any more
            return FindType(name, appId);
        }

        // Note: only public because we're still supporting a very old API in a 2sxc code
        public Type FindType(string name, int appId)
        {
            // first try to just find the type, but check if it's marked [Obsolete]
            var type = Type.GetType(name);
            if (type != null && !type.GetCustomAttributes(typeof(ObsoleteAttribute), inherit: false).Any())
                return type;

            // if not found, or if obsolete, try to find another
            var typeFromCatalog = FindInCache(name, appId)?.Type;
            return typeFromCatalog ?? type;
        }


        /// <summary>
        /// Get all Installed DataSources
        /// </summary>
        /// <remarks>Objects that implement IDataSource</remarks>
        public IEnumerable<DataSourceInfo> GetAll(bool onlyForVisualQuery, int appId)
        {
            var fromGlobal = onlyForVisualQuery
                ? GlobalCache.Where(dsi => (dsi.VisualQuery?.GlobalName).HasValue())
                : GlobalCache;

            var appList = Get(appId) ?? new List<DataSourceInfo>();
            var fromApp = onlyForVisualQuery
                ? appList.Where(dsi => (dsi.VisualQuery?.GlobalName).HasValue())
                : appList;

            return fromGlobal.Concat(fromApp);
        }
    }
}
