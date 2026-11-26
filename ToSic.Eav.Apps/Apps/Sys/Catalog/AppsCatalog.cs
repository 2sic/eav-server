using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Apps.Sys.Loaders;

namespace ToSic.Eav.Apps.Sys.Catalog;

/// <summary>
/// This is the implementation of States which doesn't use the static Eav.Apps.State
/// It's not final, so please keep very internal
/// The names of the Get etc. will probably change a few more times
/// </summary>
[PrivateApi("internal")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppsCatalog(AppsCacheSwitch appsCacheSwitch, LazySvc<IAppLoaderTools> appLoaderTools) : IAppsCatalog
{
    internal readonly AppsCacheSwitch AppsCacheSwitch = appsCacheSwitch;

    public IAppIdentityPure AppIdentity(int appId)
        => new AppIdentityPure(AppsCacheSwitch.Value.ZoneIdOfApp(appId, appLoaderTools.Value.RepositoryLoader(null)), appId);

    public IAppIdentityPure PrimaryAppIdentity(int zoneId)
        => new AppIdentityPure(zoneId, Zones[zoneId].PrimaryAppId);

    public IAppIdentityPure DefaultAppIdentity(int zoneId)
        => new AppIdentityPure(zoneId, Zones[zoneId].DefaultAppId);

    public string AppNameId(IAppIdentity appIdentity)
        => Zones[appIdentity.ZoneId].Apps[appIdentity.AppId];

    public IReadOnlyDictionary<int, string> Apps(int zoneId)
        => Zones[zoneId].Apps;

    [field: AllowNull, MaybeNull]
    public IReadOnlyDictionary<int, Zone> Zones => field
        ??= AppsCacheSwitch.Value.Zones(appLoaderTools.Value.RepositoryLoader(null));

    public Zone Zone(int zoneId)
        => Zones.TryGetValue(zoneId, out var zone)
            ? zone
            : throw new ArgumentOutOfRangeException(nameof(zoneId), zoneId, $@"Zone {zoneId} found");
}
