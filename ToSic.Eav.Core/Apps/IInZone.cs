using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// This things belongs to a zone
    /// </summary>
    [PublicApi]
    public interface IInZone
    {
        /// <summary>
        /// ID of the zone (EAV Tenant)
        /// </summary>
        /// <returns>The zone ID this thing belongs to</returns>
        int ZoneId { get; }
    }
}
