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
public class AppsCatalog(IAppsCacheSelector appsCacheSelector, LazySvc<IAppLoaderTools> appLoaderTools) : IAppsCatalog
{
    public IAppIdentityPure AppIdentity(int appId)
        => new AppIdentityPure(appsCacheSelector.Value.ZoneIdOfApp(appId, appLoaderTools.Value.RepositoryLoader()), appId);

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
        ??= appsCacheSelector.Value.Zones(appLoaderTools.Value.RepositoryLoader());

    public Zone Zone(int zoneId)
        => Zones.TryGetValue(zoneId, out var zone)
            ? zone
            : throw new ArgumentOutOfRangeException(nameof(zoneId), zoneId, $@"Zone {zoneId} found");
}
