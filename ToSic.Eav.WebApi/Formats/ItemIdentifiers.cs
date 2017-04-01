using System;

namespace ToSic.Eav.WebApi.Formats
{

    public class ItemIdentifier
    {
        // simple entity identifier (to edit existing)...
        public int EntityId { get; set; }

        // the Guid
        public Guid Guid { get; set; } // not 

        // ...or content-type (for new)
        public string ContentTypeName { get; set; }

        #region Additional Assignment information
        public Metadata Metadata { get; set; }
        #endregion
        public GroupAssignment Group { get; set; }

        // this is not needed on the server, but must be passed through so it's still attached to this item if in use
        public dynamic Prefill { get; set; }
        public string Title { get; set; }

        public int? DuplicateEntity { get; set; }
    }

    public class EntityWithHeader
    {
        public ItemIdentifier Header { get; set; }
        public EntityWithLanguages Entity { get; set; }
    }

    public class GroupAssignment
    {
        public Guid Guid { get; set; }

        /// <summary>
        /// The Set is either "content" or "listcontent", "presentation" or "listpresentation"
        /// </summary>
        public string Part { get; set; }

        public int Index { get; set; }

        /// <summary>
        /// "Add" informs the save-routine that it is an additional slot
        /// </summary>
        public bool Add { get; set; }

 
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

        //public bool ContentBlockIsEntity { get; set; }
        //public int ContentBlockId { get; set; }
        public int ContentBlockAppId { get; set; }
    }

}
