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
            var type = FindDataSourceInfo(name, appId)?.Type;
            if (type == null) return name;
            var longName = type.AssemblyQualifiedName;
            var first = longName.IndexOf(',');
            var second = longName.IndexOf(',', first + 2);
            var newName = longName.Substring(0, second);
            return newName;
        }
        // 2023-03-21 2dm disabled this as we now need the DSI
        //internal Type FindTypeByGuidOrName(string name, int appId)
        //{
        //    // New 11.12.x If the type is identified by a GUID, that's what we should return
        //    var typeInfo = FindInCache(name, appId);
        //    if (typeInfo?.Name != null) return typeInfo.Type;

        //    // Old mechanism which checks real types etc but probably is never needed any more
        //    return FindDataSourceInfo(name, appId).Type;
        //}
        internal DataSourceInfo FindDsiByGuidOrName(string name, int appId)
        {
            // New 11.12.x If the type is identified by a GUID, that's what we should return
            var typeInfo = FindInCache(name, appId);
            if (typeInfo?.Name != null) return typeInfo;

            // Old mechanism which checks real types etc but probably is never needed any more
            return FindDataSourceInfo(name, appId);
        }

        // 2023-03-21 2dm disabled this as we now need the DSI
        //// Note: only public because we're still supporting a very old API in a 2sxc code
        //public Type FindType(string name, int appId)
        //{
        //    // first try to just find the type, but check if it's marked [Obsolete]
        //    var type = Type.GetType(name);
        //    if (type != null && !type.GetCustomAttributes(typeof(ObsoleteAttribute), inherit: false).Any())
        //        return type;

        //    // if not found, or if obsolete, try to find another
        //    var typeFromCatalog = FindInCache(name, appId)?.Type;
        //    return typeFromCatalog ?? type;
        //}
        // Note: only public because we're still supporting a very old API in a 2sxc code
        public DataSourceInfo FindDataSourceInfo(string name, int appId)
        {
            // 2023-03-31 2dm disabled this as we now need the DSI - believe this should not have an effect
            //// first try to just find the type, but check if it's marked [Obsolete]
            //var type = Type.GetType(name);
            //if (type != null && !type.GetCustomAttributes(typeof(ObsoleteAttribute), inherit: false).Any())
            //    return type;

            // if not found, or if obsolete, try to find another
            var typeFromCatalog = FindInCache(name, appId);
            return typeFromCatalog;
        }

        /// <summary>
        /// Get all Installed DataSources
        /// </summary>
        /// <remarks>Objects that implement IDataSource</remarks>
        public IEnumerable<DataSourceInfo> GetAll(bool onlyForVisualQuery, int appId)
        {
            var fromGlobal = onlyForVisualQuery
                ? GlobalCache.Where(dsi => (dsi.VisualQuery?.NameId).HasValue())
                : GlobalCache;

            var appList = Get(appId) ?? new List<DataSourceInfo>();
            var fromApp = onlyForVisualQuery
                ? appList.Where(dsi => (dsi.VisualQuery?.NameId).HasValue()).ToList()
                : appList;

            return fromGlobal.Concat(fromApp);
        }
    }
}
