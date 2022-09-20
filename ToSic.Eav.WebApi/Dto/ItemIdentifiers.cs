using System;
using Newtonsoft.Json;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.Formats
{

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

        #region Old properties only for support of the old UI, will be deprecated soon
        /// <summary>
        /// Group information, for items which are coming from a group and not using direct IDs
        /// This also contains information about 
        /// </summary>
        public GroupAssignment Group { get; set; }

        #endregion

        //// this is not needed on the server, but must be passed through so it's still attached to this item if in use
        public dynamic Prefill { get; set; }


        public int? DuplicateEntity { get; set; }

        #region New features in 11.01 adding things in lists

        /// <summary>
        /// Experimental 11.01
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Field { get; set; }

        /// <summary>
        /// Experimental 11.01
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Guid? Parent { get; set; }

        /// <summary>
        /// Experimental 11.01 - move from Group to here
        /// </summary>
        public bool? Add
        {
            // 2022-09-19 2dm - WIP, part of #cleanUpDuplicateGroupHeaders
            get => _add/* ?? Group?.Add*/;
            set => _add = value;
        }
        private bool? _add;

        /// <summary>
        /// Experimental 11.01 - move from Group to here
        /// </summary>
        public int? Index { get; set; }

        // 2022-09-20 stv #cleanUpDuplicateGroupHeaders - WIP
        public bool ListHas() => IsContentBlockMode/*Group != null*/ || Parent != null;

        // 2022-09-20 stv #cleanUpDuplicateGroupHeaders - WIP
        public Guid ListParent() => /*Group?.Guid ??*/ Parent 
            ?? throw new ArgumentNullException(nameof(Parent),"Trying to access property 'Parent' to save in list, but it's null");
        public int ListIndex(int fallback = 0) => /*Group?.Index ??*/ Index ?? fallback; // Fallback should be the max value

        // 2022-09-19 2dm - WIP, part of #cleanUpDuplicateGroupHeaders
        public bool ListAdd() => /*Group?.Add ??*/ Add ?? false;

        // 2022-09-20 stv #cleanUpDuplicateGroupHeaders - WIP
        public int? ListContentBlockAppId() => Group?.ContentBlockAppId ?? ContentBlockAppId;

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
        /// WIP in v14.09 - to replace Group.SlotCanBeEmpty
        /// </remarks>
        public bool IsEmptyAllowed { get; set; }

        /// <summary>
        /// LeaveBlank means that the slot - no matter if new or existing - should be blank and should NOT contain the entity
        /// It may even mean that the slot must be blanked now
        /// </summary>
        /// <remarks>
        /// WIP in 14.09 - to replace Group.SlotIsEmpty
        /// </remarks>
        public bool IsEmpty { get; set; }

        // 2022-09-20 stv #cleanUpDuplicateGroupHeaders - WIP
        /// <summary>
        /// If it's a content-block
        /// </summary>
        [JsonIgnore]
        public bool IsContentBlockMode { get; set; } = false;

        // 2022-09-20 stv #cleanUpDuplicateGroupHeaders - WIP
        /// <summary>
        /// ContentBlockAppId is currently not used by UI.
        /// It is possible that is required to solve "inner-inner-content" issue.
        /// Moved from Group.ContentBlockAppId, because "Group" properties are flattened to its parent "Header".
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
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

    public class GroupAssignment
    {
        /// <summary>
        /// Entity Guid of the group
        /// </summary>
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public Guid? Guid { get; set; } // 2022-09-20 stv #cleanUpDuplicateGroupHeaders - WIP


        /// <summary>
        /// The Set is either "content" or "listcontent", "presentation" or "listpresentation"
        /// </summary>
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public string Part { get; set; } // 2022-09-20 stv #cleanUpDuplicateGroupHeaders - WIP

        /// <summary>
        /// The index (position) in the group)
        /// </summary>
        /// <remarks>
        /// We know that there is a small risk here, 
        /// because if two people work on the same item the index could be off if a person adds items 
        /// and the other person still viewing the last list (with different index).
        /// It's low risk, so we won't address this ATM.
        /// </remarks>
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public int? Index { get; set; } // 2022-09-20 stv #cleanUpDuplicateGroupHeaders - WIP

        // 2022-09-19 2dm - WIP, part of #cleanUpDuplicateGroupHeaders
        ///// <summary>
        ///// "Add" informs the save-routine that it is an additional slot which should be saved
        ///// </summary>
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public bool? Add { get; set; } // 2022-09-20 stv #cleanUpDuplicateGroupHeaders - WIP

        ///// <summary>
        ///// Determines that an empty slot is allowed / possible
        ///// This will usually affect the UI in the possible options
        ///// </summary>
        //public bool SlotCanBeEmpty { get; set; }

        ///// <summary>
        ///// LeaveBlank means that the slot - no matter if new or existing - should be blank and should NOT contain the entity
        ///// It may even mean that the slot must be blanked now
        ///// </summary>
        //public bool SlotIsEmpty { get; set; }

        // 2022-09-20 stv #cleanUpDuplicateGroupHeaders - WIP
        /// <summary>
        /// ContentBlockAppId is currently not used by UI.
        /// It is possible that is required to solve "inner-inner-content" issue.
        /// Moved to its parent "Header", because "Group" properties are flattened, but still not deleted here.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ContentBlockAppId { get; set; } // 2022-09-20 stv #cleanUpDuplicateGroupHeaders - WIP
    }

}
