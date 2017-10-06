using System;

namespace ToSic.Eav.Interfaces
{
	/// <summary>
	/// Represents a light Entity
	/// </summary>
	public interface IEntityLight
	{
        /// <summary>
        /// New test value - let the entity know where it belongs to, making it complete and allowing further data lookup if necessary...
        /// </summary>
        int AppId { get; }

        /// <summary>
        /// Gets the EntityId
        /// </summary>
        int EntityId { get; }

		/// <summary>
		/// Gets the EntityGuid
		/// </summary>
		Guid EntityGuid { get; }


        IMetadata Metadata { get; }


		/// <summary>
		/// Gets the ContentType of this Entity
		/// </summary>
		IContentType Type { get; }

		/// <summary>
		/// Gets the "official" Title-Attribute
		/// </summary>
		object Title { get; }

		/// <summary>
		/// Gets the Last Modified DateTime
		/// </summary>
		DateTime Modified { get; }

		/// <summary>
		/// Gets an Attribute by its StaticName
		/// </summary>
		/// <param name="attributeName">StaticName of the Attribute</param>
		object this[string attributeName] { get; }

        /// <summary>
        /// Relationship-helper object, important to navigate to children and parents
		/// </summary>
        IRelationshipManager Relationships { get; }

	    /// <summary>
	    /// Retrieves the best possible value for an attribute or virtual attribute (like EntityTitle)
	    /// Automatically resolves the language-variations as well based on the list of preferred languages
	    /// </summary>
	    /// <param name="attributeName">Name of the attribute or virtual attribute</param>
	    /// <param name="resolveHyperlinks"></param>
	    /// <returns>
	    /// An object OR a null - for example when retrieving the title and no title exists
		/// the object is string, int or even a EntityRelationship
		/// </returns>
		object GetBestValue(string attributeName, bool resolveHyperlinks = false);

        /// <summary>
        /// Best way to get the current entities title
        /// </summary>
        /// <returns>The entity title as a string</returns>
        string GetBestTitle();

        /// <summary>
        /// Owner of this entity
        /// </summary>
        string Owner { get; }
	}
}