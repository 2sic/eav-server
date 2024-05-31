namespace ToSic.Eav.Caching;

/// <summary>
/// Experimental - to make dependencies on global cache id more explicit
/// and use cache IDs without keeping a reference to the object itself. 
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface ICanBeCacheDependency
{
    /// <summary>
    /// Identifier in the notify-cache system.
    ///
    /// For objects which are kind of global (e.g. features) it should be the full namespace.
    /// For objects which are standalone - e.g. Apps, it should be something unique (e.g. namespace) + AppId.
    /// </summary>
    public string CacheId { get; }

}