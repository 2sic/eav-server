using System.Collections.Concurrent;

namespace ToSic.Eav.Apps.Sys.Caching;

internal class AppsCacheStateInMemory : IAppsCacheState
{
    public const string DefaultNameId = "MemoryCache";

    /// <summary>
    /// Identifies this cache to check if the one currently used matches this one.
    /// </summary>
    public string NameId => DefaultNameId;

    public ConcurrentDictionary<string, Lazy<IReadOnlyDictionary<int, Zone>>> ZonesWithApps => ZoneAppCaches;

    public ConcurrentDictionary<string, IAppStateCache> AppStates => Caches;

    /// <summary>
    /// Per-tenant zone cache keyed by runtime key.
    /// </summary>
    private static readonly ConcurrentDictionary<string, Lazy<IReadOnlyDictionary<int, Zone>>> ZoneAppCaches = new();

    /// <summary>
    /// Global app-state cache shared by all instances; entries are runtime-key scoped.
    /// </summary>
    private static readonly ConcurrentDictionary<string, IAppStateCache> Caches = new();

}
