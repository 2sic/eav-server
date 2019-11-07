using System.Collections.Generic;
using ToSic.Eav.Documentation;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Data
{
	/// <summary>
	/// Represents a Content Type information (the schema) used for <see cref="IEntity"/> objects.
	/// </summary>
	[PublicApi]
	public interface IContentType
	{
        /// <summary>
        /// The app to which this content type belongs
        /// </summary>
        int AppId { get; }

        /// <summary>
        /// Gets the Display Name of the Content Type
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Static name - can be a GUID or a system-term for special types
		/// </summary>
        string StaticName { get; }

        /// <summary>
        /// The content-type description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Get the scope of the Content Type (like sections in a DB)
        /// </summary>
        string Scope { get; }

        /// <summary>
        /// Get the id of the Content Type - you usually don't need this!
        /// </summary>
        int ContentTypeId { get; }

        /// <summary>
        /// Dictionary with all Attribute Definitions
        /// </summary>
        IList<IAttributeDefinition> Attributes { get; set; }

        /// <summary>
        /// A simple indexer to get an attribute
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns>The <see cref="IAttributeDefinition"/> of the field name</returns>
        IAttributeDefinition this[string fieldName] { get; }

        /// <summary>
        /// Information where the Content-Type was stored (file system, DB, etc.)
        /// </summary>
        RepositoryTypes RepositoryType { get; }

        /// <summary>
        /// Information / ID / URL to this content-type where it was stored in the repository
        /// </summary>
        string RepositoryAddress { get; }

        /// <summary>
        /// Determines if the data for this type is dynamic (stored as JSON) or EAV (split into sql-tables)
        /// </summary>
        bool IsDynamic { get; }

        /// <summary>
        /// Get the metadata for this content-type
        /// </summary>
        /// <remarks>
        /// The metadata is either already prepared, from the same app, or from a remote app
        /// </remarks>
        ContentTypeMetadata Metadata { get; }

        /// <summary>
        /// Check if this type is the same as a name given.
        /// Will check both the name and the static name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
	    bool Is(string name); 
	}
}
