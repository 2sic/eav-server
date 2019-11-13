using System;
using ToSic.Eav.Documentation;
using ToSic.Eav.Metadata;
using ToSic.Eav.Security.Permissions;

namespace ToSic.Eav.Data
{
    /// <inheritdoc cref="IAttributeBase" />
    /// <summary>
    /// Represents an Attribute definition in a ContentType. This is the base for attributes in <see cref="IContentType"/>
    /// </summary>
    [PublicApi]
    public interface IAttributeDefinition: IAttributeBase, IHasPermissions
	{
        /// <summary>
        /// AppId for this attribute
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

        /// <summary>
        /// Additional information, specs etc. about this attribute
        /// </summary>
        IMetadataOfItem Metadata { get; }

        [PrivateApi]
	    [Obsolete("this is the old call - which returns unknown. for the new UI, we should use the #2, which will later replace this")]
        string InputType { get; }

        [PrivateApi]
        // 2018-08-26 2dm new version temporary, will later replace the InputType
	    string InputTypeTempBetterForNewUi { get; }

	}
}
