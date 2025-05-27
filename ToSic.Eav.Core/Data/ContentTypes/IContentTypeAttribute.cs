using ToSic.Eav.Apps;
using ToSic.Eav.Metadata;
using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Data;

/// <summary>
/// Represents an Attribute definition in a ContentType. This is the base for attributes in <see cref="IContentType"/>
/// </summary>
[PublicApi]
public interface IContentTypeAttribute: IAttributeBase, IHasPermissions, IAppIdentityLight, IHasMetadata
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
    /// Attribute GUID to uniquely identify this attribute if it is being shared with other attributes.
    /// #SharedFieldDefinition
    /// </summary>
    /// <remarks>
    /// Created ca. v16, releasing ca. v18.02
    /// </remarks>
    [PrivateApi]
    Guid? Guid { get; }

    /// <summary>
    /// System Settings for this attribute, mainly for field-definition sharing and inheriting.
    /// #SharedFieldDefinition
    /// </summary>
    /// <remarks>
    /// Created ca. v16, releasing ca. v18.02
    /// </remarks>
    [PrivateApi]
    ContentTypeAttributeSysSettings SysSettings { get; }
}