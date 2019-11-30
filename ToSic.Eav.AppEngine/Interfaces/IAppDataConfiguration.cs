using ToSic.Eav.Documentation;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// The configuration of an app-data - usually relevant so the source will auto-filter out unpublished data for normal viewers.
    /// </summary>
    [PublicApi]
    public interface IAppDataConfiguration
    {
        /// <summary>
        /// If this instance is allowed to show draft items
        /// This is usually dependent on the current users permissions
        /// </summary>
        bool ShowDrafts { get; }

        /// <summary>
        /// If data-versioning is currently enabled
        /// </summary>
        bool PublishingEnabled { get; }

        /// <summary>
        /// Configuration used to query data - will deliver url-parameters and other important configuration values.
        /// </summary>
        ITokenListFiller Configuration { get; }
    }
}
