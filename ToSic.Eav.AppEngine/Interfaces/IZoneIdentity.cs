namespace ToSic.Eav.Apps.Interfaces
{
    /// <summary>
    /// This things belongs to a zone
    /// </summary>
    [PublicApi.PublicApi]
    public interface IZoneIdentity
    {
        /// <summary>
        /// ID of the zone (EAV Tenant)
        /// </summary>
        /// <returns>The zone ID</returns>
        int ZoneId { get; }
    }
}
