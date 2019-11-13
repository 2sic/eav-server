using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// This thing belongs to an App
    /// </summary>
    [PublicApi]
    public interface IInApp
    {
        /// <summary>
        /// The app id as used internally
        /// </summary>
        /// <returns>The App ID this thing belongs to</returns>
        int AppId { get; }
    }
}
