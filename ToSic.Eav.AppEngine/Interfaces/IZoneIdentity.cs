namespace ToSic.Eav.Apps.Interfaces
{
    /// <summary>
    /// A zone
    /// </summary>
    public interface IZoneIdentity
    {
        /// <summary>
        /// ID of the zone (EAV Tenant)
        /// </summary>
        int ZoneId { get; }
    }
}
