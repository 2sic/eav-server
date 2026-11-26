namespace ToSic.Eav.Apps.Sys.Caching;

/// <summary>
/// This will retrieve the current AppCache.
/// Basically it will get all registered <see cref="IAppsCacheManager"/> and get the first which says it's valid.
/// Once it's found the right one, it will keep returning that,
/// unless the features report that a setting has been changed, in which case it will re-evaluate the best match. 
/// </summary>
/// <remarks>
/// This class is **scoped** since the value provided changes very rarely, only on settings changes.
/// </remarks>
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppsCacheSelector
{
    /// <summary>
    /// Get the current AppsCache - using the name/key from the settings
    /// so that it could be overloaded.
    /// </summary>
    IAppsCacheManager Value { get; }
}
