using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
	/// <summary>
	/// Represents a Content Type which is available somewhere, but is defined elsewhere
	/// </summary>
	[PrivateApi("not yet ready to publish, names will probably change some day")]
	public interface IContentTypeShared
    {
        /// <summary>
        /// The parent zone
        /// </summary>
        int ParentZoneId { get; }

        /// <summary>
        /// The parent app
        /// </summary>
        int ParentAppId { get; }

        /// <summary>
        /// Get the id of the source Content Type if configuration is used from another
        /// </summary>
        int? ParentId { get; }

        /// <summary>
        /// If this configuration is auto-shared everywhere
        /// </summary>
        bool AlwaysShareConfiguration { get; }

    }
}
