using System.Text.Json.Serialization;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Metadata;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Data.Entities.Sys;

/// <inheritdoc />
/// <remarks>
/// Not 100% #immutable, because the EntityId is still manipulated once in case it's a draft-entity of another entity.
/// Not sure when/how to fix.
/// </remarks>
[PrivateApi("2021-09-30 hidden now, previously InternalApi_DoNotUse_MayChangeWithoutNotice this is just fyi, always use IEntity")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract record EntityLight //: IEntityLight
{
    #region Basic properties EntityId, EntityGuid, Title, Attributes, Type, Modified, etc.

    /// <inheritdoc />
    public required int AppId { get; init; }

    /// <inheritdoc />
    public required int EntityId { get; set; } // NOTE: not nice implementation, as it has a set; but won't bleed as the interface only has get

    /// <inheritdoc />
    public required Guid EntityGuid { get; init; }

    [JsonIgnore]
    [PrivateApi]
    internal /*required*/ string TitleFieldName
    {
        get => field ?? Type.TitleFieldName;
        init;
    }

    /// <inheritdoc />
    public required IContentType Type { get; init; }

    /// <inheritdoc />
    public required DateTime Created { get; init; }
        
    /// <inheritdoc />
    public required DateTime Modified { get; init; }

    public required EntityPartsLazy PartsLazy { get; init; }

    /// <inheritdoc />
    [JsonIgnore]
    public IEntityRelationships Relationships => field ??= PartsLazy.GetRelationshipDelegate((IEntity)this);


    /// <inheritdoc />
    public required ITarget MetadataFor
    {
        get => field ??= new Target();
        init;
    }

    /// <inheritdoc />
    public required string Owner { get; init; }

    /// <inheritdoc />
    public int OwnerId => _ownerId.Get(() => int.TryParse(Owner.After("="), out var o) ? o : -1);
    private readonly GetOnce<int> _ownerId = new();
    #endregion


    #region GetBestValue and GetTitle

    /// <summary>
    /// Get internal properties by string-name like "EntityTitle", etc.
    /// Resolves: EntityId, EntityGuid, EntityType, EntityModified
    /// Also ensure that it works in any upper/lower case
    /// </summary>
    /// <param name="attributeNameLowerInvariant"></param>
    /// <returns></returns>
    [PrivateApi]
    protected virtual object GetInternalPropertyByName(string attributeNameLowerInvariant) =>
        attributeNameLowerInvariant switch
        {
            AttributeNames.EntityFieldId => EntityId,
            AttributeNames.EntityFieldGuid => EntityGuid,
            AttributeNames.EntityFieldType => Type.Name,
            AttributeNames.EntityFieldCreated => Created,
            AttributeNames.EntityFieldOwner => Owner, // added in v15, was missing before
            AttributeNames.EntityFieldOwnerId => OwnerId, // new v15.03
            AttributeNames.EntityFieldModified => Modified,
            AttributeNames.EntityAppId => AppId,
            _ => null
        };

    #endregion

}