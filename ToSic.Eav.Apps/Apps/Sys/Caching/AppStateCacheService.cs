using ToSic.Eav.Apps.Sys.State;

namespace ToSic.Eav.Apps.Sys.Caching;

/// <summary>
/// This is the implementation of States which doesn't use the static Eav.Apps.State
/// It's not final, so please keep very internal
/// The names of the Get etc. will probably change a few more times
/// </summary>
[PrivateApi("internal")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppStateCacheService(IAppsCacheSelector appsCacheSelector)
    : IAppStateCacheService
{
    /// <inheritdoc />
    public IAppStateCache Get(IAppIdentity app)
        => appsCacheSelector.Value.Get(app);

    /// <inheritdoc />
    public IAppStateCache Get(int appId)
        => appsCacheSelector.Value.Get(AppIdentity(appId));

    public bool IsCached(IAppIdentity appId)
        => appsCacheSelector.Value.Has(appId);

    private IAppIdentityPure AppIdentity(int appId)
    {
        var zoneId = appsCacheSelector.Value.ZoneIdOfApp(appId);
        return new AppIdentityPure(zoneId, appId);
    }
}
