namespace ToSic.Eav.Interfaces
{
	/// <summary>
	/// Represents an Attribute. This is the base for both
	/// - attribute definition (in the IContentType)
	/// - attribute with values-list (in the IEntity)
	/// </summary>
	public interface IAttributeDefinition: IAttributeBase
	{
        int AppId { get; }

        // additional info for the persistence layer
        int AttributeId { get; }

        int SortOrder { get; }

        bool IsTitle { get; }

    }
}
