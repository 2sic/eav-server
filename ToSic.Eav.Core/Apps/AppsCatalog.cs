namespace ToSic.Eav.Apps;

internal class AppsCatalog(AppStates appStates) : IAppsCatalog
{
    public IReadOnlyDictionary<int, Zone> Zones => appStates.Zones;

    public Zone Zone(int zoneId) => appStates.Zones.TryGetValue(zoneId, out var zone)
        ? zone
        : throw new ArgumentOutOfRangeException(nameof(zoneId), zoneId, $@"Zone {zoneId} found");

    public IDictionary<int, string> Apps(int zoneId) => appStates.Apps(zoneId);

    public IAppIdentityPure DefaultAppIdentity(int zoneId) => appStates.DefaultAppIdentity(zoneId);

    public IAppIdentityPure PrimaryAppIdentity(int zoneId) => appStates.PrimaryAppIdentity(zoneId);

    public IAppIdentityPure AppIdentity(int appId) => appStates.AppIdentity(appId);

    public string AppNameId(int zoneId, int appId) => appStates.AppNameId(zoneId, appId);

}