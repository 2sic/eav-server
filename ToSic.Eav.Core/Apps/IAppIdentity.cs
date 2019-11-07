using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
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
