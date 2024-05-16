namespace ToSic.Eav.Apps;

/// <summary>
/// Marks things which belongs to a Zone
/// </summary>
[PublicApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IZoneIdentity
{
    /// <summary>
    /// ID of the zone (EAV Tenant)
    /// </summary>
    /// <returns>The zone ID this thing belongs to</returns>
    int ZoneId { get; }
}