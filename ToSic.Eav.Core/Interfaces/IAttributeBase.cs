namespace ToSic.Eav.Interfaces
{
	/// <summary>
	/// Represents an Attribute. This is the base for both
	/// - attribute definition (in the IContentType)
	/// - attribute with values-list (in the IEntity)
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
    }
}
