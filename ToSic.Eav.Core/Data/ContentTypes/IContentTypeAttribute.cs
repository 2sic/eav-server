using ToSic.Eav.Apps;
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
    public interface IContentTypeAttribute: IAttributeBase, IHasPermissions, IInApp
	{
        ///// <summary>
        ///// AppId for this attribute
        ///// </summary>
        //int AppId { get; }

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
        IMetadataOf Metadata { get; }

	}
}
