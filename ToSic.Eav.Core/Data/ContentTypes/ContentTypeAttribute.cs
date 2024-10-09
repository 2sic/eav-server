using ToSic.Eav.Metadata;
using ToSic.Eav.Security;

namespace ToSic.Eav.Data;

/// <summary>
/// Defines an attribute with name and the type this attribute has.
/// Part of of a <see cref="IContentType"/> definition.
/// </summary>
/// <remarks>
/// * completely #immutable since v15.04
/// </remarks>
[PrivateApi("2021-09-30 changed to private, before was internal-this is just fyi, always use the interface")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ContentTypeAttribute(
    int appId,
    string name,
    ValueTypes type,
    bool isTitle,
    int attributeId,
    int sortOrder,
    Guid? guid,
    ContentTypeAttributeSysSettings sysSettings = default,
    IMetadataOf metadata = default)
    : AttributeBase(name, type), IContentTypeAttribute
{
    /// <inheritdoc />
    public int AppId { get; } = appId;

    /// <inheritdoc />
    public int AttributeId { get; } = attributeId;

    /// <inheritdoc />
    public int SortOrder { get; } = sortOrder;

    /// <inheritdoc />
    public bool IsTitle { get; } = isTitle;

    [PrivateApi] // #SharedFieldDefinition
    public Guid? Guid { get; } = guid;

    /// <summary>
    /// SysSettings for this attribute - can be null!
    /// #SharedFieldDefinition
    /// </summary>
    [PrivateApi]
    public ContentTypeAttributeSysSettings SysSettings { get; } = sysSettings;


    #region Metadata and Permissions

    /// <inheritdoc />
    public IMetadataOf Metadata { get; } = metadata;

    /// <inheritdoc />
    [PrivateApi("because permissions will probably become an entity-based type")]
    public IEnumerable<Permission> Permissions => Metadata.Permissions;

    #endregion

}