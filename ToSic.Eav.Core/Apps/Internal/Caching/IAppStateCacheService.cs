using ToSic.Eav.Apps.State;

namespace ToSic.Eav.Apps.Internal;

[PrivateApi("WIP")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppStateCacheService
{
    /// <summary>
    /// Retrieve an app as is cached.
    /// Will trigger cache build for this app if it's not yet in the cache.
    /// </summary>
    /// <param name="app">App identifier.</param>
    IAppStateCache Get(IAppIdentity app);

    /// <summary>
    /// Retrieve an app from the cache
    /// </summary>
    /// <param name="appId">App id if zone unknown.</param>
    IAppStateCache Get(int appId);

    /// <summary>
    /// Test if the app is already in the cache.
    /// Used by loaders etc. which need to inspect something without triggering cache build.
    /// </summary>
    /// <param name="appId"></param>
    /// <returns></returns>
    bool IsCached(IAppIdentity appId);
}
