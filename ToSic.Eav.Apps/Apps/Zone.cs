using ToSic.Eav.Data;
using ToSic.Eav.Data.Dimensions.Sys;

namespace ToSic.Eav.Apps;

/// <summary>
/// Contains all the basic infos about a Zone - usually cached
/// </summary>
[PrivateApi("was Internal till 16.09, but no need to show implementation")]
public class Zone(int zoneId, int primaryAppId, int contentAppId, IReadOnlyDictionary<int, string> apps, List<DimensionDefinition> languages) : IZoneIdentity
{
    /// <inheritdoc />
    public int ZoneId { get; internal set; } = zoneId;

    /// <summary>
    /// AppId of the default App in this Zone
    /// </summary>
    public int DefaultAppId { get; } = contentAppId;

    /// <summary>
    /// The Primary App which also contains Settings and shared Metadata
    /// WIP #SiteApp v13
    /// </summary>
    public int PrimaryAppId { get; } = primaryAppId;

    /// <summary>
    /// All Apps in this Zone with Id and Name
    /// </summary>
    public IReadOnlyDictionary<int, string> Apps { get; internal set; } = apps;

    /// <summary>
    /// Languages available in this Zone
    /// </summary>
    public List<DimensionDefinition> Languages { get; } = languages;

    public List<DimensionDefinition> LanguagesActive => field ??= Languages.Where(l => l.Active).ToList();
}