using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Interfaces
{
    /// <inheritdoc cref="IZoneIdentity" />
    /// <summary>
    /// A app object capable of telling us it's identity
    /// </summary>
    [PublicApi.PublicApi]
    public interface IAppIdentity: IZoneIdentity, ICanLogState
    {
        /// <summary>
        /// The app id as used internally
        /// </summary>
        /// <returns>The App ID</returns>
        int AppId { get; }
    }
}
