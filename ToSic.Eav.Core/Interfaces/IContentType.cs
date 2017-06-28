using System.Collections.Generic;

namespace ToSic.Eav.Interfaces
{
	/// <summary>
	/// Represents a Content Type
	/// </summary>
	public interface IContentType
	{

        int AppId { get; }

        /// <summary>
        /// Gets the Display Name of the Content Type
        /// </summary>
        string Name { get; }
		/// <summary>
		/// Gets the Static Name of the Content Type
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
        /// Get the id of the Content Type (AttributeSet)
        /// </summary>
        int ContentTypeId { get; }

        /// <summary>
        /// Dictionary with all Attribute Definitions
        /// </summary>
        IList<IAttributeDefinition> Attributes { get; set; }

        // A simple indexer to get an attribute
        IAttributeDefinition this[string fieldName] { get; }

    }
}
