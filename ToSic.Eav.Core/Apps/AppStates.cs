using ToSic.Eav.Apps.State;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;

namespace ToSic.Eav.Apps;

/// <summary>
/// This is the implementation of States which doesn't use the static Eav.Apps.State
/// It's not final, so please keep very internal
/// The names of the Get etc. will probably change a few more times
/// </summary>
[PrivateApi("internal")]
internal class AppStates(AppsCacheSwitch appsCacheSwitch)
    : IAppStates
{
    internal readonly AppsCacheSwitch AppsCacheSwitch = appsCacheSwitch;

    /// <inheritdoc />
    public IAppStateCache Get(IAppIdentity app)
        => AppsCacheSwitch.Value.Get(app, AppsCacheSwitch.AppLoaderTools);

    /// <inheritdoc />
    public IAppStateCache GetCacheState(int appId)
        => AppsCacheSwitch.Value.Get(IdentityOfApp(appId), AppsCacheSwitch.AppLoaderTools);

    public bool IsCached(IAppIdentity appId)
        => AppsCacheSwitch.Value.Has(appId);

    public IAppIdentityPure IdentityOfApp(int appId)
        => new AppIdentityPure(AppsCacheSwitch.Value.ZoneIdOfApp(appId, AppsCacheSwitch.AppLoaderTools), appId);

    public IAppIdentityPure IdentityOfPrimary(int zoneId)
        => new AppIdentityPure(zoneId, PrimaryAppId(zoneId));

    public IAppIdentityPure IdentityOfDefault(int zoneId)
        => new AppIdentityPure(zoneId, DefaultAppId(zoneId));

    public string AppIdentifier(int zoneId, int appId)
        => AppsCacheSwitch.Value.Zones(AppsCacheSwitch.AppLoaderTools)[zoneId].Apps[appId];

    public int DefaultAppId(int zoneId)
        => AppsCacheSwitch.Value.Zones(AppsCacheSwitch.AppLoaderTools)[zoneId].DefaultAppId;

    public int PrimaryAppId(int zoneId)
        => AppsCacheSwitch.Value.Zones(AppsCacheSwitch.AppLoaderTools)[zoneId].PrimaryAppId;

    public IDictionary<int, string> Apps(int zoneId)
        => AppsCacheSwitch.Value.Zones(AppsCacheSwitch.AppLoaderTools)[zoneId].Apps;

    public List<DimensionDefinition> Languages(int zoneId, bool includeInactive = false)
    {
        var zone = AppsCacheSwitch.Value.Zones(AppsCacheSwitch.AppLoaderTools)[zoneId];
        return includeInactive ? zone.Languages : zone.Languages.Where(l => l.Active).ToList();
    }

    public IReadOnlyDictionary<int, Zone> Zones => AppsCacheSwitch.Value.Zones(AppsCacheSwitch.AppLoaderTools);
}