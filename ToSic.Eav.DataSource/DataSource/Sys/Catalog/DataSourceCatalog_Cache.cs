﻿using ToSic.Eav.DataSource.VisualQuery.Sys;
using ToSic.Sys.Utils.Assemblies;

namespace ToSic.Eav.DataSource.Sys.Catalog;

partial class DataSourceCatalog
{
    /// <summary>
    /// A cache of all DataSource Types - initialized upon first access ever, then static cache.
    /// </summary>
    private static List<DataSourceInfo> GlobalCache { get; } = AssemblyHandling
        .FindInherited(typeof(IDataSource))
        .Select(t => new DataSourceInfo(t, true))
        // TODO: ORDER IN A WAY THAT OBSOLETE ONES ARE LAST
        .ToList();

    /// <summary>
    /// Find a DataSource which may have changed it's name. Will look in the cached names list.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="appId"></param>
    /// <returns></returns>
    private DataSourceInfo? FindInCache(string name, int appId)
    {
        var list = GlobalCache;
        var inGlobal = FindInCachedList(name, list, false);
        if (inGlobal != null)
            return inGlobal;

        // New v15.04 - also support app level data sources
        var appLocalDataSources = Get(appId);
        return appLocalDataSources == null
            ? null 
            : FindInCachedList(name, appLocalDataSources, true);
    }

    private static DataSourceInfo? FindInCachedList(string name, List<DataSourceInfo> list, bool isLocal)
        => // First check for normal type name
            list.FirstOrDefault(dst => dst.NameId.EqualsInsensitive(name))
            ?? (isLocal ? list.FirstOrDefault(dst => dst.TypeName.EqualsInsensitive(name)) : null)
            // Otherwise check for historical names in the VisualQuery Attribute
            ?? list.FirstOrDefault(dst =>
                dst.VisualQuery?.NameIds.Any(pn => pn.EqualsInsensitive(name)) ?? false);
}