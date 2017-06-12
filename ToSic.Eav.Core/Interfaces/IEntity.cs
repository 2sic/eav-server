using System;
using System.Collections.Generic;

namespace ToSic.Eav.Interfaces
{
	/// <summary>
	/// Represents an Entity
	/// </summary>
	public interface IEntity
	{
		/// <summary>
		/// Gets the EntityId
		/// </summary>
		int EntityId { get; }
		/// <summary>
		/// Gets the RepositoryId
		/// </summary>
		int RepositoryId { get; }

		/// <summary>
		/// Gets the EntityGuid
		/// </summary>
		Guid EntityGuid { get; }

		/// <summary>
		/// Gets the AssignmentObjectTypeId
		/// </summary>
		int AssignmentObjectTypeId { get; }

        IMetadata Metadata { get; }
		/// <summary>
		/// Gets a Dictionary having all Attributes having a value
		/// </summary>
		Dictionary<string, IAttribute> Attributes { get; }
		/// <summary>
		/// Gets the ContentType of this Entity
		/// </summary>
		IContentType Type { get; }
		/// <summary>
		/// Gets the Title-Attribute
		/// </summary>
		IAttribute Title { get; }
		/// <summary>
		/// Gets the Last Modified DateTime
		/// </summary>
		DateTime Modified { get; }
		/// <summary>
		/// Gets an Attribute by its StaticName
		/// </summary>
		/// <param name="attributeName">StaticName of the Attribute</param>
		IAttribute this[string attributeName] { get; }
		/// <summary>
		/// Get Related entities
		/// </summary>
		IRelationshipManager Relationships { get; }
		/// <summary>
		/// Indicates whether this Entity is Published (true) or a Draft (false)
		/// </summary>
		bool IsPublished { get; }
		/// <summary>
		/// Get Draft Entity of this Entity
		/// </summary>
		IEntity GetDraft();
		/// <summary>
		/// Get Published Entity of this Entity
		/// </summary>
		IEntity GetPublished();

		/// <summary>
		/// Helper method to retrieve the most applicable value based on criteria like current language etc.
		/// </summary>
		/// <param name="attributeName"></param>
		/// <param name="resolveHyperlinks"></param>
		/// <returns>A string, int or even a EntityRelationship</returns>
		object GetBestValue(string attributeName, bool resolveHyperlinks = false);
		object GetBestValue(string attributeName, string[] dimensions, bool resolveHyperlinks = false);

        /// <summary>
        /// Best way to get the current entities title
        /// </summary>
        /// <returns>The entity title as a string</returns>
        string GetBestTitle();
        string GetBestTitle(string[] dimensions);

        /// <summary>
        /// Owner of this entity
        /// </summary>
        string Owner { get; }
	}
}