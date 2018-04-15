namespace ToSic.Eav.Interfaces
{
    /// <inheritdoc cref="IAttributeBase" />
    /// <summary>
    /// Represents an Attribute. This is the base for both
    /// - attribute definition (in the IContentType)
    /// - attribute with values-list (in the IEntity)
    /// </summary>
    public interface IAttributeDefinition: IAttributeBase//, IHasMetadata
	{
        /// <summary>
        /// AppId
        /// </summary>
        int AppId { get; }

        /// <summary>
        /// additional info for the persistence layer
        /// </summary>
        int AttributeId { get; }

        /// <summary>
        /// position of this attribute in the list of attributes
        /// </summary>
        int SortOrder { get; }

        /// <summary>
        /// tells us if this attribute is the title
        /// </summary>
        bool IsTitle { get; }

        IMetadataOfItem Metadata { get; }

        string InputType { get; }
	}
}
