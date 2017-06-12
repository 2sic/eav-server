namespace ToSic.Eav.Interfaces
{
	/// <summary>
	/// Represents a Content Type
	/// </summary>
	public interface IContentTypeShareable
    {

        int ParentConfigurationZoneId { get; }
        int ParentConfigurationAppId { get; }

        /// <summary>
        /// Get the id of the source Content Type if configuration is used from another
        /// </summary>
        int? ParentConfigurationId { get; }

        bool AlwaysShareConfiguration { get; }

    }
}
