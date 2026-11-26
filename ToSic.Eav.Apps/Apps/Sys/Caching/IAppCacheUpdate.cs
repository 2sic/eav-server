namespace ToSic.Eav.Apps.Sys.Caching;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppCacheUpdate
{
    /// <summary>
    /// Request the cache to reload some entities.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="entities"></param>
    void Update(IAppIdentity app, IEnumerable<int> entities);
}