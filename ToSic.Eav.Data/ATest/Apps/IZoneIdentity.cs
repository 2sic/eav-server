namespace ToSic.Eav.Apps;

/// <summary>
/// Marks things which belongs to a Zone
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice] // was public till v18, but certainly never used.
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IZoneIdentity
{
    /// <summary>
    /// ID of the zone (EAV Tenant)
    /// </summary>
    /// <returns>The zone ID this thing belongs to</returns>
    int ZoneId { get; }
}