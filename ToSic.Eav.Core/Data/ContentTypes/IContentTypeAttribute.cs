using ToSic.Eav.Apps;
using ToSic.Eav.Documentation;
using ToSic.Eav.Metadata;
using ToSic.Eav.Security;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents an Attribute definition in a ContentType. This is the base for attributes in <see cref="IContentType"/>
    /// </summary>
    [PublicApi]
    public interface IContentTypeAttribute: IAttributeBase, IHasPermissions, IInApp
	{
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
