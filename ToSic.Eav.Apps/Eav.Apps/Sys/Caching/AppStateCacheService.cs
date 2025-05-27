using ToSic.Eav.Apps.State;
using ToSic.Eav.Caching;

namespace ToSic.Eav.Apps.Internal;

/// <summary>
/// This is the implementation of States which doesn't use the static Eav.Apps.State
/// It's not final, so please keep very internal
/// The names of the Get etc. will probably change a few more times
/// </summary>
[PrivateApi("internal")]
public class AppStateCacheService(AppsCacheSwitch appsCacheSwitch) : IAppStateCacheService
{
    internal readonly AppsCacheSwitch AppsCacheSwitch = appsCacheSwitch;

    /// <inheritdoc />
    public IAppStateCache Get(IAppIdentity app)
        => AppsCacheSwitch.Value.Get(app, AppsCacheSwitch.AppLoaderTools);

    /// <inheritdoc />
    public IAppStateCache Get(int appId)
        => AppsCacheSwitch.Value.Get(AppIdentity(appId), AppsCacheSwitch.AppLoaderTools);

    public bool IsCached(IAppIdentity appId)
        => AppsCacheSwitch.Value.Has(appId);

    private IAppIdentityPure AppIdentity(int appId)
        => new AppIdentityPure(AppsCacheSwitch.Value.ZoneIdOfApp(appId, AppsCacheSwitch.AppLoaderTools), appId);
}