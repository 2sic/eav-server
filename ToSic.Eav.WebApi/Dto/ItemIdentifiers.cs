using System;
using Newtonsoft.Json;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.WebApi.Dto;
using IEntity = ToSic.Eav.Data.IEntity;

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
            get => _add ?? Group?.Add;
            set => _add = value;
        }
        private bool? _add;

        /// <summary>
        /// Experimental 11.01 - move from Group to here
        /// </summary>
        public int? Index { get; set; }

        public bool ListHas() => Group != null || Parent != null;
        public Guid ListParent() => Group?.Guid ?? Parent.Value;
        public int ListIndex() => Group?.Index ?? Index.Value;

        public bool ListAdd() => Group?.Add ?? Add ?? false;

        #endregion

        #region New EditInfo for v13 / Shared Apps

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public EditInfoDto EditInfo { get; set; }

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
        public Guid Guid { get; set; }

        /// <summary>
        /// The Set is either "content" or "listcontent", "presentation" or "listpresentation"
        /// </summary>
        public string Part { get; set; }

        /// <summary>
        /// The index (position) in the group)
        /// </summary>
        /// <remarks>
        /// We know that there is a small risk here, 
        /// because if two people work on the same item the index could be off if a person adds items 
        /// and the other person still viewing the last list (with different index).
        /// It's low risk, so we won't address this ATM.
        /// </remarks>
        public int Index { get; set; }

        /// <summary>
        /// "Add" informs the save-routine that it is an additional slot which should be saved
        /// </summary>
        public bool Add { get; set; }

        ///// <summary>
        ///// This property is only needed by the new UI, because it does more checking
        ///// and because the previous information if it should really, really add (entityId=0) fails
        ///// with the new API since it's set upon saving
        ///// </summary>
        //[JsonIgnore]
        //public bool? ReallyAddBecauseAlreadyVerified { get; set; }
 
        /// <summary>
        /// Determines that an empty slot is allowed / possible
        /// This will usually affect the UI in the possible options
        /// </summary>
        public bool SlotCanBeEmpty { get; set; }

       /// <summary>
        /// LeaveBlank means that the slot - no matter if new or existing - should be blank and should NOT contain the entity
        /// It may even mean that the slot must be blanked now
        /// </summary>
        public bool SlotIsEmpty { get; set; }

        public int ContentBlockAppId { get; set; }
    }

}
