using ToSic.Eav.ValueProvider;
using ToSic.Eav.ValueProviders;

namespace ToSic.Eav.Apps.Interfaces
{
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
        bool VersioningEnabled { get; }

        /// <summary>
        /// Configuration used to query data
        /// </summary>
        IValueCollectionProvider Configuration { get; }
    }
}
