using System;
using System.Collections.Generic;

namespace ToSic.Eav.Interfaces
{
	/// <summary>
	/// Represents an Entity
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

        // 2017-06-13 2dm - try to disable this - I assume it's only used internally
		///// <summary>
		///// Gets the AssignmentObjectTypeId
		///// </summary>
		//int AssignmentObjectTypeId { get; }

        IMetadata Metadata { get; }


		///// <summary>
		///// Gets a Dictionary having all Attributes having a value
		///// </summary>
		//Dictionary<string, object> Attributes { get; }

		/// <summary>
		/// Gets the ContentType of this Entity
		/// </summary>
		IContentType Type { get; }
		/// <summary>
		/// Gets the Title-Attribute
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
		/// Get Related entities
		/// </summary>
		IRelationshipManager Relationships { get; }

		/// <summary>
		/// Helper method to retrieve the most applicable value based on criteria like current language etc.
		/// </summary>
		/// <param name="attributeName"></param>
		/// <param name="resolveHyperlinks"></param>
		/// <returns>A string, int or even a EntityRelationship</returns>
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