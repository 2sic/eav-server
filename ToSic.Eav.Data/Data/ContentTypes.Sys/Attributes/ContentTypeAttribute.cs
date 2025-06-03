using ToSic.Eav.Metadata;
using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Data;

/// <summary>
/// Defines an attribute with name and the type this attribute has.
/// Part of a <see cref="IContentType"/> definition.
/// </summary>
/// <remarks>
/// * completely #immutable since v15.04
/// * Changed to be a record in v19.01
/// </remarks>
[PrivateApi("2021-09-30 changed to private, before was internal-this is just fyi, always use the interface")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record ContentTypeAttribute : AttributeBase, IContentTypeAttribute
{
    /// <inheritdoc />
    public required int AppId { get; init; }

    /// <inheritdoc />
    public required int AttributeId { get; init; }

    /// <inheritdoc />
    public required int SortOrder { get; init; }

    /// <inheritdoc />
    public required bool IsTitle { get; init; }

    [PrivateApi] // #SharedFieldDefinition
    public required Guid? Guid { get; init; }

    /// <summary>
    /// SysSettings for this attribute - can be null!
    /// #SharedFieldDefinition
    /// </summary>
    [PrivateApi]
    public required ContentTypeAttributeSysSettings SysSettings { get; init; }


    #region Metadata and Permissions

    /// <inheritdoc />
    public required IMetadataOf Metadata { get; init; }

    /// <inheritdoc />
    [PrivateApi("Not public yet, as it's not quite clear what the permissions affect and how to communicate this. Reason is that some affect file access, others have different purposes")]
    public IEnumerable<IPermission> Permissions => Metadata.Permissions;

    #endregion

}