using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Interfaces
{
    /// <inheritdoc cref="IZoneIdentity" />
    /// <summary>
    /// A app object capable of telling us it's identity
    /// </summary>
    public interface IAppIdentity: IZoneIdentity, ICanLogState
    {
        /// <summary>
        /// The app id as used internally
        /// </summary>
        int AppId { get; }
    }
}
