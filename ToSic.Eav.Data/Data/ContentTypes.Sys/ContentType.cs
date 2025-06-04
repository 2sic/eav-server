using System.Collections.Immutable;
using System.Text.Json.Serialization;
using ToSic.Eav.Metadata;
using ToSic.Eav.Repositories;
using ToSic.Lib.Helpers;
using ToSic.Sys.Caching.PiggyBack;

namespace ToSic.Eav.Data.ContentTypes.Sys;

/// <summary>
/// Represents a ContentType
/// </summary>
// Remarks: Before 2021-09 it was marked as PublicApi
// We should actually make it PrivateApi, but other code references this, so we need to change that to IContentType,
// Otherwise docs won't generate cross-links as needed
[PrivateApi("2021-09-30 hidden now, was internal_don't use Always use the interface, not this class")]
[ShowApiWhenReleased(ShowApiMode.Never)]
[ContentTypeSpecs(
    Guid = "e405beb3-9097-4790-b7b0-0e6d37502bef",
    Name = "ContentType",
    Scope = "System",
    Description = "A ContentType (Schema) describing Entities."
)]
public partial record ContentType : IContentType, IContentTypeShared, IHasDecorators<IContentType>, IHasPiggyBack
{
    #region simple properties - all are #immutable

    /// <inheritdoc />
    public required int AppId { get; init; }

    /// <inheritdoc />
    [ContentTypeAttributeSpecs(IsTitle = true)]
    public required string Name { get; init; }

    /// <inheritdoc />
    [Obsolete("Deprecated in v13, please use NameId instead")]
    [ContentTypeAttributeIgnore]
    public string StaticName => NameId;

    /// <inheritdoc />
    public required string NameId { get; init; }

    /// <inheritdoc />
    public required string Scope { get; init; }

    /// <inheritdoc />
    public required int Id { get; init; }

    /// <inheritdoc />
    [Obsolete("Deprecated in V13, please use Id instead.")]
    [ContentTypeAttributeIgnore]
    public int ContentTypeId => Id;

    /// <inheritdoc />
    [ContentTypeAttributeIgnore]
    public IEnumerable<IContentTypeAttribute> Attributes => AttributesImmutable;
    public required IImmutableList<IContentTypeAttribute> AttributesImmutable { get; init; }

    /// <inheritdoc />
    [ContentTypeAttributeSpecs(Type = ValueTypes.String)]
    public required RepositoryTypes RepositoryType { get; init; }

    /// <inheritdoc />
    public required string RepositoryAddress { get; init; }

    /// <inheritdoc />
    public required bool IsDynamic { get; init; }

    #endregion

    /// <inheritdoc />
    public bool Is(string name)
        => Name.EqualsInsensitive(name) || NameId.EqualsInsensitive(name);

    [JsonIgnore]
    [PrivateApi("new 15.04")]
    [ContentTypeAttributeIgnore]
    public string TitleFieldName
        => _titleFieldName.Get(() => Attributes.FirstOrDefault(a => a.IsTitle)?.Name);
    private readonly GetOnce<string> _titleFieldName = new();

    /// <summary>
    /// For future use, like if this type is SQL based etc.
    /// </summary>
    [PrivateApi]
    [ContentTypeAttributeIgnore]
    public ContentTypeSysSettings SysSettings => null;

    /// <inheritdoc />
    public IContentTypeAttribute this[string fieldName]
        => Attributes.FirstOrDefault(a => a.Name.EqualsInsensitive(fieldName));


    #region New DynamicChildren Navigation - new in 12.03 - #immutable

    /// <inheritdoc />
    [PrivateApi("WIP 12.03")]
    // Don't cache the result, as it could change during runtime
    // TODO: probably remove and use the DetailsOrNull2() instead
    public string DynamicChildrenField => this.DetailsOrNull()?.DynamicChildrenField;

    #endregion


    #region Advanced Properties: Metadata, Decorators - all #immutable

    /// <inheritdoc />
    public required IMetadataOf Metadata { get; init; }

    IMetadataOf IHasMetadata.Metadata => Metadata;

    // Decorators - note that ATM we don't seem to use them
    public required IEnumerable<IDecorator<IContentType>> Decorators { get; init; }


    #endregion

    #region Sharing Content Types - all #immutable

    public required bool AlwaysShareConfiguration { get; init; }

    #endregion

    /// <summary>
    /// Improve ToString for better debugging.
    /// </summary>
    public override string ToString() => $"{this.Name}/{NameId} - {base.ToString()}";

    public PiggyBack PiggyBack => field ??= new();
}