using ToSic.Eav.Apps.State;
using ToSic.Eav.Caching;

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

    public IAppsCatalog AppsCatalog => _appsCatalog ??= new(this);
    private AppsCatalog _appsCatalog;

    /// <inheritdoc />
    public IAppStateCache Get(IAppIdentity app)
        => AppsCacheSwitch.Value.Get(app, AppsCacheSwitch.AppLoaderTools);

    /// <inheritdoc />
    public IAppStateCache GetCacheState(int appId)
        => AppsCacheSwitch.Value.Get(AppIdentity(appId), AppsCacheSwitch.AppLoaderTools);

    public bool IsCached(IAppIdentity appId)
        => AppsCacheSwitch.Value.Has(appId);

    public IAppIdentityPure AppIdentity(int appId)
        => new AppIdentityPure(AppsCacheSwitch.Value.ZoneIdOfApp(appId, AppsCacheSwitch.AppLoaderTools), appId);

    public IAppIdentityPure PrimaryAppIdentity(int zoneId)
        => new AppIdentityPure(zoneId, PrimaryAppId(zoneId));

    public IAppIdentityPure DefaultAppIdentity(int zoneId)
        => new AppIdentityPure(zoneId, DefaultAppId(zoneId));

    internal string AppNameId(int zoneId, int appId)
        => Zones[zoneId].Apps[appId];

    public int DefaultAppId(int zoneId)
        => Zones[zoneId].DefaultAppId;

    public int PrimaryAppId(int zoneId)
        => Zones[zoneId].PrimaryAppId;

    public IDictionary<int, string> Apps(int zoneId)
        => Zones[zoneId].Apps;

    //public List<DimensionDefinition> Languages(int zoneId, bool includeInactive = false)
    //{
    //    var zone = Zones[zoneId];
    //    return includeInactive ? zone.Languages : zone.Languages.Where(l => l.Active).ToList();
    //}

    public IReadOnlyDictionary<int, Zone> Zones => _zones ??= AppsCacheSwitch.Value.Zones(AppsCacheSwitch.AppLoaderTools);
    private IReadOnlyDictionary<int, Zone> _zones;
}