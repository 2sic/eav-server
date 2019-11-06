using System;
using ToSic.Eav.Documentation;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Interfaces
{
	/// <summary>
	/// Represents a light Entity, which is a very basic entity
	/// without multi-language capabilities, versions or publishing.
	/// For the more powerful Entity, use <see cref="IEntity"/>.
	/// </summary>
	[PublicApi]
	public interface IEntityLight
	{
        /// <summary>
        /// New test value - let the entity know where it belongs to, making it complete and allowing further data lookup if necessary...
        /// </summary>
        /// <returns>The App ID this entity belongs to.</returns>
        int AppId { get; }

        /// <summary>
        /// Gets the EntityId
        /// </summary>
        /// <returns>The internal EntityId - usually for reference in the DB, but not quite always (like when this is a draft entity).</returns>
        int EntityId { get; }

		/// <summary>
		/// Gets the EntityGuid
		/// </summary>
		/// <returns>The GUID of the Entity</returns>
		Guid EntityGuid { get; }

        /// <summary>
        /// Information which is relevant if this current entity is actually mapped to something else.
        /// If it is mapped, then it's describing another thing, which is identified in this MetadataFor.
        /// </summary>
        /// <returns>A <see cref="IMetadataFor"/> object describing the target.</returns>
        IMetadataFor MetadataFor { get; }

		/// <summary>
		/// Gets the ContentType of this Entity
		/// </summary>
		/// <returns>The content-type object.</returns>
		IContentType Type { get; }

		/// <summary>
		/// Gets the "official" Title-Attribute <see cref="IAttribute{T}"/>
		/// </summary>
		/// <returns>
		/// The title of this Entity.
		/// The field used is determined in the <see cref="IContentType"/>.
		/// If you need a string, use GetBestTitle() instead.
		/// </returns>
		object Title { get; }

		/// <summary>
		/// Gets the Last Modified DateTime
		/// </summary>
		/// <returns>A date-time object.</returns>
		DateTime Modified { get; }

		/// <summary>
		/// Gets an Attribute by its StaticName
		/// </summary>
		/// <param name="attributeName">StaticName of the Attribute</param>
		/// <returns>The attribute - probably an <see cref="IAttribute{T}"/> </returns>
		object this[string attributeName] { get; }

        /// <summary>
        /// Relationship-helper object, important to navigate to children and parents
		/// </summary>
		/// <returns>The <see cref="IRelationshipManager"/> in charge of relationships for this Entity.</returns>
        IRelationshipManager Relationships { get; }

	    /// <summary>
	    /// Retrieves the best possible value for an attribute or virtual attribute (like EntityTitle)
	    /// Automatically resolves the language-variations as well based on the list of preferred languages
	    /// </summary>
	    /// <param name="attributeName">Name of the attribute or virtual attribute</param>
	    /// <param name="resolveHyperlinks">If true, will try to resolve links in the value. Default is false.</param>
	    /// <returns>
	    /// An object OR a null - for example when retrieving the title and no title exists
		/// the object is string, int or even a EntityRelationship
		/// </returns>
		object GetBestValue(string attributeName, bool resolveHyperlinks = false);


        /// <summary>
        /// Retrieves the best possible value for an attribute or virtual attribute (like EntityTitle)
        /// Automatically resolves the language-variations as well based on the list of preferred languages.
        /// Will cast/convert to the expected type, or return null / default value for that type if not possible.
        /// </summary>
        /// <param name="name">Name of the attribute or virtual attribute</param>
        /// <param name="resolveHyperlinks">If true, will try to resolve links in the value. Default is false.</param>
        /// <returns>
        /// An object OR a null - for example when retrieving the title and no title exists
        /// the object is string, int or even a EntityRelationship
        /// </returns>
	    TVal GetBestValue<TVal>(string name, bool resolveHyperlinks = false);

        /// <summary>
        /// Best way to get the current entities title.
        /// The field used is determined in the <see cref="IContentType"/>.
        /// If you need the attribute-object, use the <see cref="Title"/> instead.
        /// </summary>
        /// <returns>
        /// The entity title as a string.
        /// </returns>
        string GetBestTitle();

        /// <summary>
        /// Owner of this entity
        /// </summary>
        /// <returns>A string identifying the owner. Uses special encoding to work with various user-ID providers.</returns>
        string Owner { get; }
	}
}