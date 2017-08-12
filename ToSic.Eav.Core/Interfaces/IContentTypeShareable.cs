namespace ToSic.Eav.Interfaces
{
	/// <summary>
	/// Represents a Content Type
	/// </summary>
	public interface IContentTypeShareable
    {

        int ParentZoneId { get; }
        int ParentAppId { get; }

        /// <summary>
        /// Get the id of the source Content Type if configuration is used from another
        /// </summary>
        int? ParentId { get; }

        bool AlwaysShareConfiguration { get; }

    }
}
