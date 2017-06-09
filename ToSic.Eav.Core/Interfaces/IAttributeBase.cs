using System.Collections.Generic;

namespace ToSic.Eav
{
	/// <summary>
	/// Represents an Attribute
	/// </summary>
	public interface IAttributeBase
	{
		/// <summary>
		/// Name of the Attribute
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Type of the Attribute
		/// </summary>
		string Type { get; }
        AttributeTypeEnum ControlledType { get; }

        bool IsTitle { get; }

        // additional info for the persistence layer
        int AttributeId { get; }

        int SortOrder { get; }

    }
}
