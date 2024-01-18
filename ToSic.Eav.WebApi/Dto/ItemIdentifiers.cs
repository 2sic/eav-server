using ToSic.Eav.ImportExport.Json.V1;

namespace ToSic.Eav.WebApi.Formats;

public class ItemIdentifier
{
    /// <summary>
    /// simple entity identifier (to edit existing)...
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    ///  the Guid
    /// </summary>
    public Guid Guid { get; set; } 

    /// <summary>
    /// the content-type (for new, and finding all fields etc.)
    /// </summary>
    public string ContentTypeName { get; set; }

    /// <summary>
    /// Metadata key information
    /// </summary>
    public JsonMetadataFor For { get; set; }

    /// <summary>
    /// Prefill information for the UI to add values to new / empty fields
    /// This is not needed on the server, but must be passed through so it's still attached to this item if in use
    /// </summary>
    public dynamic Prefill { get; set; }

    /// <summary>
    /// Additional data to preserve during client requests.
    /// The contents of which is not important for the server,
    /// but the client should get it again on the identifier bundle.
    /// </summary>
    /// <remarks>Added v16.01</remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object> ClientData { get; set; }

    public int? DuplicateEntity { get; set; }

    #region New features in 11.01 adding things in lists

    /// <summary>
    /// The Parent of this item - when an item is anchored in a list of another item.
    /// Used in combination with <see cref="Field"/> and <see cref="Index"/>
    /// 
    /// Used in ContentBlock scenarios (where an item is in the content/presentation field)
    /// or when editing / adding things to an entity-list like slides in a swiper.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? Parent { get; set; }

    /// <summary>
    /// The field on the parent where this item is anchored. <see cref="Parent"/>
    /// Must be an Entity-List. 
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Field { get; set; }


    /// <summary>
    /// Information if the item should be added to the list or not.
    /// It shouldn't be added if it was already there to begin with. 
    /// </summary>
    public bool? Add { get; set; }

    /// <summary>
    /// Safely access the Add property, with default to false
    /// </summary>
    [JsonIgnore]
    public bool AddSafe => Add ?? false;

    /// <summary>
    /// Position of an item inside a field containing an entity list. <see cref="Parent"/>
    /// </summary>
    public int? Index { get; set; }

    public int IndexSafeOrFallback(int fallback = 0) => Index ?? fallback; // Fallback should be the max value


    #endregion

    #region New EditInfo for v13 / Shared Apps

    public EditInfoDto EditInfo { get; set; }

    #endregion


    #region Move Group Fields to here

    /// <summary>
    /// Determines that an empty slot is allowed / possible
    /// This will usually affect the UI in the possible options
    /// </summary>
    /// <remarks>
    /// Added in v14.09 to replace Group.SlotCanBeEmpty
    /// </remarks>
    public bool IsEmptyAllowed { get; set; }

    /// <summary>
    /// LeaveBlank means that the slot - no matter if new or existing - should be blank and should NOT contain the entity
    /// It may even mean that the slot must be blanked now
    /// </summary>
    /// <remarks>
    /// Added in 14.09 to replace Group.SlotIsEmpty
    /// </remarks>
    public bool IsEmpty { get; set; }

    /// <summary>
    /// If it's a content-block.
    /// Internal property so that we can detect this early on, and then re-use the status. 
    /// </summary>
    [JsonIgnore]
    public bool IsContentBlockMode { get; set; } = false;

    /// <summary>
    /// ContentBlockAppId is currently not used by UI.
    /// It is possible that is required to solve "inner-inner-content" issue.
    /// Moved from Group.ContentBlockAppId, because "Group" properties are flattened to its parent "Header".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ContentBlockAppId { get; set; }

    #endregion
}

public class BundleWithHeader
{
    public ItemIdentifier Header { get; set; }     
}

public class BundleWithHeader<TEntity>: BundleWithHeader
{
    public TEntity Entity { get; set; }

}

public static class ItemIdentifierExtension
{
    /// <summary>
    /// Access the Parent GUID in scenarios where it must exist, or throw error
    /// </summary>
    /// <remarks>
    /// ParentOrError property was converted to extension method to avoid exceptions on STJ json deserialization
    /// </remarks>
    public static Guid GetParentEntityOrError(this ItemIdentifier itemIdentifier)
    {
        return itemIdentifier.Parent
               ?? throw new ArgumentNullException(nameof(itemIdentifier.Parent),
                   "Trying to access 'Parent' to save in list, but it's null");
    }
}