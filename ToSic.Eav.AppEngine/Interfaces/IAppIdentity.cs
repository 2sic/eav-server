using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Interfaces
{
    /// <summary>
    /// This thing belongs to an App (which also always belongs to a zone)
    /// </summary>
    [PublicApi]
    public interface IAppIdentity: IZoneIdentity
    {
        /// <summary>
        /// The app id as used internally
        /// </summary>
        /// <returns>The App ID</returns>
        int AppId { get; }
    }
}
