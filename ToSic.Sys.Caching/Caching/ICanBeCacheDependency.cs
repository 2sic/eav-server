namespace ToSic.Eav.Caching;

/// <summary>
/// Experimental - to make dependencies on global cache id more explicit
/// and use cache IDs without keeping a reference to the object itself. 
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ICanBeCacheDependency
{
    /// <summary>
    /// If true, the cache is only used to notify other caches, but not to store the object itself.
    ///
    /// The difference will affect the key generated and checked.
    /// </summary>
    public bool CacheIsNotifyOnly { get; }


    /// <summary>
    /// Identifier in the notify-cache system.
    ///
    /// For objects which are kind of global (e.g. features) it should be the full namespace.
    /// For objects which are standalone - e.g. Apps, it should be something unique (e.g. namespace) + AppId.
    /// </summary>
    public string CacheDependencyId { get; }

}