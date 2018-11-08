using System;
using ToSic.Eav.Security.Permissions;

namespace ToSic.Eav.Interfaces
{
    /// <inheritdoc cref="IAttributeBase" />
    /// <summary>
    /// Represents an Attribute. This is the base for both
    /// - attribute definition (in the IContentType)
    /// - attribute with values-list (in the IEntity)
    /// </summary>
    public interface IAttributeDefinition: IAttributeBase, IHasPermissions
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

	    [Obsolete("this is the old call - which returns unknown. for the new UI, we should use the #2, which will later replace this")]
        string InputType { get; }

        // 2018-08-26 2dm new version temporary, will later replace the InputType
	    string InputTypeTempBetterForNewUi { get; }

	}
}
