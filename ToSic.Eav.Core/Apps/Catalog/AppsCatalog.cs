using ToSic.Eav.Apps.State;
using ToSic.Eav.Caching;

namespace ToSic.Eav.Apps.Catalog;

/// <summary>
/// This is the implementation of States which doesn't use the static Eav.Apps.State
/// It's not final, so please keep very internal
/// The names of the Get etc. will probably change a few more times
/// </summary>
[PrivateApi("internal")]
internal class AppsCatalog(AppsCacheSwitch appsCacheSwitch) : IAppsCatalog
{
    internal readonly AppsCacheSwitch AppsCacheSwitch = appsCacheSwitch;

    /// <inheritdoc />
    public IAppStateCache Get(IAppIdentity app)
        => AppsCacheSwitch.Value.Get(app, AppsCacheSwitch.AppLoaderTools);

    public IAppIdentityPure AppIdentity(int appId)
        => new AppIdentityPure(AppsCacheSwitch.Value.ZoneIdOfApp(appId, AppsCacheSwitch.AppLoaderTools), appId);

    public IAppIdentityPure PrimaryAppIdentity(int zoneId)
        => new AppIdentityPure(zoneId, PrimaryAppId(zoneId));

    public IAppIdentityPure DefaultAppIdentity(int zoneId)
        => new AppIdentityPure(zoneId, DefaultAppId(zoneId));

    public string AppNameId(int zoneId, int appId)
        => Zones[zoneId].Apps[appId];

    public int DefaultAppId(int zoneId)
        => Zones[zoneId].DefaultAppId;

    public int PrimaryAppId(int zoneId)
        => Zones[zoneId].PrimaryAppId;

    public IDictionary<int, string> Apps(int zoneId)
        => Zones[zoneId].Apps;

    public IReadOnlyDictionary<int, Zone> Zones => _zones ??= AppsCacheSwitch.Value.Zones(AppsCacheSwitch.AppLoaderTools);
    private IReadOnlyDictionary<int, Zone> _zones;

    public Zone Zone(int zoneId) => Zones.TryGetValue(zoneId, out var zone)
        ? zone
        : throw new ArgumentOutOfRangeException(nameof(zoneId), zoneId, $@"Zone {zoneId} found");

}