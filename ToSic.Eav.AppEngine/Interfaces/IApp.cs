namespace ToSic.Eav.Apps.Interfaces
{
    /// <summary>
    /// A app object capable of telling us it's identity
    /// </summary>
    public interface IApp
    {
        /// <summary>
        /// ID of the zone (EAV Tennant) this app belongs to
        /// </summary>
        int ZoneId { get; }

        /// <summary>
        /// The app id as used internally
        /// </summary>
        int AppId { get; }
    }
}
