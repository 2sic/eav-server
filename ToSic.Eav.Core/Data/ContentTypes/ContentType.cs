using System.Collections.Immutable;
using System.Text.Json.Serialization;
using ToSic.Eav.Metadata;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Repositories;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Data;

/// <summary>
/// Represents a ContentType
/// </summary>
// Remarks: Before 2021-09 it was marked as PublicApi
// We should actually make it PrivateApi, but other code references this, so we need to change that to IContentType,
// Otherwise docs won't generate cross-links as needed
[PrivateApi("2021-09-30 hidden now, was internal_don't use Always use the interface, not this class")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class ContentType : IContentType, IContentTypeShared
{
    #region Constructor - internal only, should only ever be called by the ContentTypeBuilder

    /// <summary>
    /// Basic initializer of ContentType class
    /// </summary>
    /// <remarks>
    /// Overload for in-memory entities
    /// </remarks>
    [PrivateApi]
    internal ContentType(
        int appId,
        int id,
        string name,
        string nameId,
        string scope,
        IImmutableList<IContentTypeAttribute> attributes,
        bool isAlwaysShared,
        bool? onSaveSortAttributes,
        string onSaveUseParentStaticName,
        RepositoryTypes repositoryType,
        string repositoryAddress,
        bool isDynamic,
        ContentTypeMetadata ctMetadata,
        IImmutableList<IDecorator<IContentType>> decorators
    )
    {
        AppId = appId;
        Id = id;
        Name = name;
        NameId = nameId ?? name;
        RepositoryType = repositoryType;
        RepositoryAddress = repositoryAddress ?? "";
        AlwaysShareConfiguration = isAlwaysShared;
        IsDynamic = isDynamic;
        Decorators = decorators;
        Metadata = ctMetadata;
        Scope = scope;
        _attributes = attributes;

        // Temporary properties which are only to specify saving rules
        // Should be moved elsewhere...
        OnSaveSortAttributes = onSaveSortAttributes ?? false;
        OnSaveUseParentStaticName = onSaveUseParentStaticName;
    }

    #endregion


    #region simple properties - all are #immutable

    /// <inheritdoc />
    public int AppId { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    [Obsolete("Deprecated in v13, please use NameId instead")]
    public string StaticName => NameId;

    /// <inheritdoc />
    public string NameId { get; }

    /// <inheritdoc />
    public string Scope { get; }

    /// <inheritdoc />
    public int Id { get; }

    /// <inheritdoc />
    [Obsolete("Deprecated in V13, please use Id instead.")]
    public int ContentTypeId => Id;

    /// <inheritdoc />
    public IEnumerable<IContentTypeAttribute> Attributes => _attributes;
    private readonly IImmutableList<IContentTypeAttribute> _attributes;

    /// <inheritdoc />
    public RepositoryTypes RepositoryType { get; }

    /// <inheritdoc />
    public string RepositoryAddress { get; }

    /// <inheritdoc />
    public bool IsDynamic { get; }

    #endregion

    /// <inheritdoc />
    public bool Is(string name) => Name.EqualsInsensitive(name) || NameId.EqualsInsensitive(name);

    [JsonIgnore]
    [PrivateApi("new 15.04")]
    public string TitleFieldName => _titleFieldName.Get(() => Attributes.FirstOrDefault(a => a.IsTitle)?.Name);

    [PrivateApi] // #SharedFieldDefinition
    public ContentTypeSysSettings SysSettings { get; }
    private readonly GetOnce<string> _titleFieldName = new();

    /// <inheritdoc />
    public IContentTypeAttribute this[string fieldName] => Attributes.FirstOrDefault(a => a.Name.EqualsInsensitive(fieldName));


    #region New DynamicChildren Navigation - new in 12.03 - #immutable

    /// <inheritdoc />
    [PrivateApi("WIP 12.03")]
    // Don't cache the result, as it could change during runtime
    public string DynamicChildrenField => Metadata.DetailsOrNull?.DynamicChildrenField;

    #endregion


    #region Advanced Properties: Metadata, Decorators - all #immutable

    /// <inheritdoc />
    public ContentTypeMetadata Metadata { get; }

    IMetadataOf IHasMetadata.Metadata => Metadata;

    // Decorators - note that ATM we don't seem to use them
    public IImmutableList<IDecorator<IContentType>> Decorators { get; }


    #endregion

    #region Sharing Content Types - all #immutable

    public bool AlwaysShareConfiguration { get; }

    #endregion

    /// <summary>
    /// Improve ToString for better debugging.
    /// </summary>
    public override string ToString() => $"{this.Name}/{NameId} - {base.ToString()}";
}