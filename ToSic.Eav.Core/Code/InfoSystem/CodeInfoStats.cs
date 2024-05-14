using System.Collections.Concurrent;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Code.InfoSystem;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class CodeInfoStats
{
    /// <summary>
    /// Empty for DI
    /// </summary>
    public CodeInfoStats() { }

    public void Register(LogStoreEntry use)
    {
        // We need the specs
        if (use?.Specs == null) return;
        if (!use.Specs.TryGetValue("AppId", out var appIdStr)) return;
        var appId = appIdStr.ConvertOrDefault<int>();
        if (appId == 0) return;
        ByAppCache[appId] = true;
    }

    public bool AppHasWarnings(int appId) => ByAppCache.ContainsKey(appId);

    private static IDictionary<int, bool> ByAppCache = new ConcurrentDictionary<int, bool>();
}