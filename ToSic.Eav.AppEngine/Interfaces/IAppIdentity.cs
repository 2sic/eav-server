namespace ToSic.Eav.Apps.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    /// A app object capable of telling us it's identity
    /// </summary>
    public interface IAppIdentity: IZoneIdentity
    {
        /// <summary>
        /// The app id as used internally
        /// </summary>
        int AppId { get; }
    }
}
