using System.Collections.Concurrent;

namespace ToSic.Eav.Apps.Sys.Caching;

/// <summary>
/// Represents the currently stored state of the cache in memory.
/// </summary>
[PrivateApi]
public interface IAppsCacheInMemory
{
    /// <summary>
    /// Lists all zones with the corresponding apps.
    /// </summary>
    /// <remarks>
    /// The key is a runtime-specific cache key, and the value is a lazy-loaded dictionary of zones.
    /// </remarks>
    ConcurrentDictionary<string, Lazy<IReadOnlyDictionary<int, Zone>>> ZonesWithApps { get; }

    /// <summary>
    /// 
    /// </summary>
    ConcurrentDictionary<string, IAppStateCache> AppStates { get; }
}
