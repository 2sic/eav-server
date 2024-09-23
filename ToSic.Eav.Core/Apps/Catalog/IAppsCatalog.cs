// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Apps;

/// <summary>
/// Global service providing information about apps and zones.
/// </summary>
[PublicApi]
public interface IAppsCatalog
{
    /// <summary>
    /// List of all apps inside a specific zone.
    /// </summary>
    IReadOnlyDictionary<int, string> Apps(int zoneId);

    /// <summary>
    /// List of all zones.
    /// </summary>
    IReadOnlyDictionary<int, Zone> Zones { get; }

    /// <summary>
    /// Information about a specific zone.
    /// </summary>
    Zone Zone(int zoneId);

    /// <summary>
    /// Get the identity of the "Default" (aka "Content") App of a Zone.
    /// </summary>
    IAppIdentityPure DefaultAppIdentity(int zoneId);

    /// <summary>
    /// Get the identity of the "Primary" (aka "Site") App of a Zone.
    /// </summary>
    IAppIdentityPure PrimaryAppIdentity(int zoneId);

    /// <summary>
    /// Get the full identity of an App by its ID.
    /// </summary>
    IAppIdentityPure AppIdentity(int appId);

    /// <summary>
    /// Get the NameId of an App - typically a GUID or in rare cases the word "Content".
    /// </summary>
    string AppNameId(IAppIdentity appIdentity);
}