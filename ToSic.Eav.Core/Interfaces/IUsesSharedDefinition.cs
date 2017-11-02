namespace ToSic.Eav.Interfaces
{
	/// <summary>
	/// Represents a Content Type
	/// </summary>
	public interface IUsesSharedDefinition
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
