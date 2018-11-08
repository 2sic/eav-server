using System;
using ToSic.Eav.ImportExport.Json.Format;
using ToSic.Eav.Interfaces;

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
        public Guid Guid { get; set; } // not 

        /// <summary>
        /// the content-type (for new, and finding all fields etc.)
        /// </summary>
        public string ContentTypeName { get; set; }

        /// <summary>
        /// Additional Assignment (MetadataFor) information - important for new, assigned entities
        /// </summary>
        public Metadata Metadata { get; set; }


        /// <summary>
        /// Group information, for items which are coming from a group and not using direct IDs
        /// This also contains information about 
        /// </summary>
        public GroupAssignment Group { get; set; }

        // this is not needed on the server, but must be passed through so it's still attached to this item if in use
        public dynamic Prefill { get; set; }
        public string Title { get; set; }

        public int? DuplicateEntity { get; set; }
    }

    public class BundleWithHeader
    {
        public ItemIdentifier Header { get; set; }     
    }

    public class BundleWithHeader<T>: BundleWithHeader
    {
        public T Entity { get; set; }

        /// <summary>
        /// Temporary solution to provide get/set EntityGuid across all expected types
        /// remove this when only then new save is being used
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public Guid EntityGuid
        {
            get
            {
                switch (Entity)
                {
                    case IEntity ent:
                        return ent.EntityGuid;
                    case EntityWithLanguages entLang:
                        return entLang.Guid;
                    case JsonEntity entJson:
                        return entJson.Guid;
                    default:
                        throw new Exception($"BundleWithHeader<T> - found unsupported type of T: {Entity.GetType().Name}");
                }

            }
        }

        [Newtonsoft.Json.JsonIgnore]
        public int EntityId
        {
            get
            {
                switch (Entity)
                {
                    case IEntity ent:
                        return ent.EntityId;
                    case EntityWithLanguages entLang:
                        return entLang.Id;
                    case JsonEntity entJson:
                        return entJson.Id;
                    default:
                        throw new Exception(
                            $"BundleWithHeader<T> - found unsupported type of T: {Entity.GetType().Name}");
                }

            }
        }
    }

    public class BundleIEntity: BundleWithHeader<IEntity> { }



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
