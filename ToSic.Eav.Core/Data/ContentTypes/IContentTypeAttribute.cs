using System;
using ToSic.Eav.Apps;
using ToSic.Eav.Metadata;
using ToSic.Eav.Security;
using ToSic.Lib.Documentation;

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

    [PrivateApi] // #SharedFieldDefinition
    Guid? Guid { get; }

    [PrivateApi] // #SharedFieldDefinition
    ContentTypeAttributeSysSettings SysSettings { get; }
}