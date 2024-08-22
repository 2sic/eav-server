namespace ToSic.Eav.Apps;

public interface IAppsCatalog
{
    // TODO: MAKE READ ONLY
    IDictionary<int, string> Apps(int zoneId);

    IReadOnlyDictionary<int, Zone> Zones { get; }

    Zone Zone(int zoneId);

    IAppIdentityPure DefaultAppIdentity(int zoneId);
    IAppIdentityPure PrimaryAppIdentity(int zoneId);
    IAppIdentityPure AppIdentity(int appId);

    string AppNameId(int zoneId, int appId);
}