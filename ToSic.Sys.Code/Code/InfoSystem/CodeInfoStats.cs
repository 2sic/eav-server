﻿using System.Collections.Concurrent;

namespace ToSic.Sys.Code.InfoSystem;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class CodeInfoStats
{
    /// <summary>
    /// Empty for DI
    /// </summary>
    public CodeInfoStats() { }

    public void Register(LogStoreEntry? use)
    {
        // We need the specs
        if (use?.Specs == null)
            return;
        if (!use.Specs.TryGetValue("AppId", out var appIdStr))
            return;
        var appId = appIdStr.ConvertOrDefault<int>();
        if (appId == 0)
            return;
        ByAppCache[appId] = true;
    }

    public bool AppHasWarnings(int appId) => ByAppCache.ContainsKey(appId);

    private static IDictionary<int, bool> ByAppCache = new ConcurrentDictionary<int, bool>();
}