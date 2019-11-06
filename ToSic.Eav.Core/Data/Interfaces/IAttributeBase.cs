using ToSic.Eav.Documentation;
using ToSic.Eav.Enums;

namespace ToSic.Eav.Data
{
	/// <summary>
	/// Represents an Attribute. This is the base for both
	/// - attribute definition (in the IContentType)
	/// - attribute with values-list (in the IEntity)
	/// </summary>
	[PublicApi]
	public interface IAttributeBase
	{
		/// <summary>
		/// Name of the Attribute
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Type of the Attribute like 'string', 'decimal' etc.
		/// </summary>
		string Type { get; }

        /// <summary>
        /// The official type, as a controlled value
        /// </summary>
        AttributeTypeEnum ControlledType { get; }

    }
}
