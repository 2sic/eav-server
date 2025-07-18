﻿using ToSic.Eav.DataSource.VisualQuery.Sys;

namespace ToSic.Eav.DataSource.Sys.Catalog;

partial class DataSourceCatalog
{
    internal string Find(string name, int appId)
    {
        // New 11.12.x If the type is identified by a GUID, that's what we should return
        var typeInfo = FindInCache(name, appId);
        if (typeInfo?.NameId != null) return typeInfo.NameId;

        // Old mechanism which checks real types etc but probably is never needed any more
        var type = FindDataSourceInfo(name, appId)?.Type;
        if (type == null)
            return name;
        var longName = type.AssemblyQualifiedName!;
        var first = longName.IndexOf(',');
        var second = longName.IndexOf(',', first + 2);
        var newName = longName.Substring(0, second);
        return newName;
    }


    internal DataSourceInfo? FindDsiByGuidOrName(string name, int appId)
    {
        var l = Log.Fn<DataSourceInfo?>($"{nameof(name)}: {name}, {nameof(appId)}: {appId}");
        // New 11.12.x If the type is identified by a GUID, that's what we should return
        var typeInfo = FindInCache(name, appId);
        if (typeInfo?.NameId != null)
            return l.Return(typeInfo, $"found in cache {typeInfo.NameId}");

        // Old mechanism which checks real types etc. but probably is never needed any more
        var typeFromCatalog = FindDataSourceInfo(name, appId);
        return l.Return(typeFromCatalog, typeFromCatalog?.NameId);
    }
    
    // Note: only public because we're still supporting a very old API in a 2sxc code
    public DataSourceInfo? FindDataSourceInfo(string name, int appId)
    {
        var l = Log.Fn<DataSourceInfo?>($"{nameof(name)}: {name}, {nameof(appId)}: {appId}");

        // if not found, or if obsolete, try to find another
        var typeFromCatalog = FindInCache(name, appId);
        return l.Return(typeFromCatalog, typeFromCatalog?.NameId);
    }

    /// <summary>
    /// Get all Installed DataSources
    /// </summary>
    /// <remarks>Objects that implement IDataSource</remarks>
    public IList<DataSourceInfo> GetAll(bool onlyForVisualQuery, int appId)
    {
        var l = Log.Fn<IList<DataSourceInfo>>($"{onlyForVisualQuery}, {appId}");

        var fromGlobal = onlyForVisualQuery
            ? GlobalCache.Where(dsi => (dsi.VisualQuery?.NameId).HasValue())
            : GlobalCache;

        var appList = Get(appId) ?? [];
        var fromApp = onlyForVisualQuery
            ? appList
                .Where(dsi => (dsi.VisualQuery?.NameId).HasValue())
                .ToList()
            : appList;

        var result = fromGlobal
            .Concat(fromApp)
            .ToListOpt();
        return l.Return(result, $"{result.Count}");
    }
}