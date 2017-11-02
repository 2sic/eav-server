using System.Collections.Generic;
using ToSic.Eav.Enums;

namespace ToSic.Eav.Interfaces
{
	/// <summary>
	/// Represents a Content Type
	/// </summary>
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
        /// Get the scope of the Content Type
        /// </summary>
        string Scope { get; }

        /// <summary>
        /// Get the id of the Content Type (AttributeSet) - you usually don't need this!
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
        /// <returns></returns>
        IAttributeDefinition this[string fieldName] { get; }


        //bool IsInstalledInPrimaryStorage { get; }
        Repositories Source { get; }

        bool IsDynamic { get; }

        /// <summary>
        /// Get the metadata for this content-type
        /// </summary>
        /// <remarks>
        /// The metadata is either already prepared, from the same app, or from a remote app
        /// </remarks>
        IMetadataOfItem Metadata { get; }
    }
}
