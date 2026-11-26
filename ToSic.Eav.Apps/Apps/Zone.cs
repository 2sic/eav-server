using ToSic.Eav.Data.Sys.Dimensions;

namespace ToSic.Eav.Apps;

/// <summary>
/// Contains all the basic infos about a Zone - usually for preserving in the cache
/// </summary>
/// <param name="ZoneId">The unique identifier of the Zone</param>
/// <param name="PrimaryAppId">The unique identifier of the Primary App in this Zone</param>
/// <param name="DefaultAppId">The unique identifier of the Default App in this Zone</param>
/// <param name="Apps">A dictionary of all Apps in this Zone with their Id and Name</param>
/// <param name="Languages">A list of languages available in this Zone</param>
[PrivateApi("was Internal till 16.09, but no need to show implementation")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record Zone(
    int ZoneId,
    int PrimaryAppId,
    int DefaultAppId,
    IReadOnlyDictionary<int, string> Apps,
    List<DimensionDefinition> Languages)
    : IZoneIdentity
{
    /// <summary>
    /// List of languages which are active.
    /// </summary>
    [field: AllowNull, MaybeNull]
    public IList<DimensionDefinition> LanguagesActive
        => field ??= Languages
            .Where(l => l.Active)
            .ToListOpt();
}
