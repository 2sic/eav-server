using System.Collections.Generic;
using ToSic.Eav.Interfaces;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav
{
	/// <summary>
	/// Represents a Content Type
	/// </summary>
	public interface IContentType
	{
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
        /// Get the id of the Content Type (AttributeSet)
        /// </summary>
        int AttributeSetId { get; }

        /// <summary>
        /// Dictionary with all Attribute Definitions
        /// </summary>
        IDictionary<int, IAttributeDefinition> AttributeDefinitions { get; set; }

        // A simple indexer to get an attribute
        IAttributeDefinition this[string fieldName] { get; }

        /// <summary>
        /// Get the scope of the Content Type
        /// </summary>
        string Scope { get; }
        
        /// <summary>
        /// Get the id of the source Content Type if configuration is used from another
        /// </summary>
        int? UsesConfigurationOfAttributeSet { get; }

        int ConfigurationAppId { get; }
        int ConfigurationZoneId { get; }

        bool ConfigurationIsOmnipresent { get; }

    }
}
