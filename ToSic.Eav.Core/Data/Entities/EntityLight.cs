using System.Collections.Immutable;
using System.Text.Json.Serialization;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Metadata;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Data;

/// <inheritdoc />
/// <remarks>
/// Not 100% #immutable, because the EntityId is still manipulated once in case it's a draft-entity of another entity.
/// Not sure when/how to fix.
/// </remarks>
[PrivateApi("2021-09-30 hidden now, previously InternalApi_DoNotUse_MayChangeWithoutNotice this is just fyi, always use IEntity")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public partial record EntityLight : IEntityLight
{
    #region Basic properties EntityId, EntityGuid, Title, Attributes, Type, Modified, etc.

    /// <inheritdoc />
    public required int AppId { get; init; }

    /// <inheritdoc />
    public required int EntityId { get; set; } // NOTE: not nice implementation, as it has a set; but won't bleed as the interface only has get

    /// <inheritdoc />
    public required Guid EntityGuid { get; init; }

    // #EntityLight-UnusedAttributes - turned off 2025-01-17 2dm, probably remove 2025-Q2
    ///// <inheritdoc />
    //public object Title => TitleFieldName.HasValue()
    //    ? this[TitleFieldName]
    //    : null;
    //private object Title => TitleFieldName.HasValue()
    //    ? AttributesLight.TryGetValue(TitleFieldName, out var result)
    //        ? result
    //        : null
    //    : null;

    [JsonIgnore]
    [PrivateApi]
    internal /*required*/ string TitleFieldName
    {
        get => field ?? Type.TitleFieldName;
        init => field = value;
    }

    // #EntityLight-UnusedAttributes - turned off 2025-01-17 2dm, probably remove 2025-Q2
    ///// <summary>
    ///// List of all attributes in light-mode - single language, simple.
    ///// Internal use only!
    ///// </summary>
    ///// <remarks>
    ///// 2dm 2025-01-17 - I believe all code paths set this to null, so it can't actually be in use.
    ///// Should consider to remove this, as apparently it's not ever used...
    ///// </remarks>
    //[PrivateApi("Internal use only!")]
    //public required IImmutableDictionary<string, object> AttributesLight { get; init; }

    /// <inheritdoc />
    public required IContentType Type { get; init; }

    /// <inheritdoc />
    public required DateTime Created { get; init; }
        
    /// <inheritdoc />
    public required DateTime Modified { get; init; }

    public required EntityPartsLazy PartsLazy { get; init; }

    /// <inheritdoc />
    [JsonIgnore]
    public IEntityRelationships Relationships => field ??= PartsLazy.GetRelationshipDelegate(this);


    /// <inheritdoc />
    public required ITarget MetadataFor { get => field ??= new Target(); init => field = value; }

    /// <inheritdoc />
    public required string Owner { get; init; }

    /// <inheritdoc />
    public int OwnerId => _ownerId.Get(() => int.TryParse(Owner.After("="), out var o) ? o : -1);
    private readonly GetOnce<int> _ownerId = new();
    #endregion

    #region direct attribute accessor using this[...]

    // #EntityLight-UnusedAttributes - turned off 2025-01-17 2dm, probably remove 2025-Q2
    ///// <inheritdoc />
    //public object this[string attributeName]
    //    => AttributesLight.TryGetValue(attributeName, out var result)
    //        ? result
    //        : null;
    #endregion



    #region GetBestValue and GetTitle

    // #EntityLight-UnusedAttributes - turned off 2025-01-17 2dm, probably remove 2025-Q2
    ///// <inheritdoc />
    //public object GetBestValue(string attributeName) 
    //{
    //    if (!AttributesLight.TryGetValue(attributeName, out var result))
    //    {
    //        var attributeNameLower = attributeName.ToLowerInvariant();
    //        if (attributeNameLower == Attributes.EntityFieldTitle)
    //            result = Title;
    //        else
    //            return GetInternalPropertyByName(attributeNameLower);
    //    }

    //    // map any kind of number to the one format used in other code-checks: decimal
    //    return result is short or ushort or int or uint or long or ulong or float or double or decimal
    //        ? Convert.ToDecimal(result)
    //        : result;
    //}


    // #EntityLight-UnusedAttributes - turned off 2025-01-17 2dm, probably remove 2025-Q2
    //[PrivateApi("Testing / wip #IValueConverter")]
    //public TVal GetBestValue<TVal>(string name) => GetBestValue(name).ConvertOrDefault<TVal>();

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
            Attributes.EntityFieldId => EntityId,
            Attributes.EntityFieldGuid => EntityGuid,
            Attributes.EntityFieldType => Type.Name,
            Attributes.EntityFieldCreated => Created,
            Attributes.EntityFieldOwner => Owner, // added in v15, was missing before
            Attributes.EntityFieldOwnerId => OwnerId, // new v15.03
            Attributes.EntityFieldModified => Modified,
            Attributes.EntityAppId => AppId,
            _ => null
        };


    // #EntityLight-UnusedAttributes - turned off 2025-01-17 2dm, probably remove 2025-Q2
    ///// <inheritdoc />
    //public string GetBestTitle() => GetBestTitle(0);

    // #EntityLight-UnusedAttributes - turned off 2025-01-17 2dm, probably remove 2025-Q2
    //private string GetBestTitle(int recursionCount)
    //{
    //    var bestTitle = GetBestValue(Attributes.EntityFieldTitle);

    //    // in case the title is an entity-picker and has items, try to ask it for the title
    //    // note that we're counting recursions, just to be sure it won't loop forever
    //    var maybeRelationship = bestTitle as IEnumerable<IEntity>;
    //    if (recursionCount < 3 && (maybeRelationship?.Any() ?? false))
    //        bestTitle = (maybeRelationship.FirstOrDefault() as Entity)?
    //                    .GetBestTitle(recursionCount + 1)
    //                    ?? bestTitle;

    //    return bestTitle?.ToString();
    //}

    #endregion

}