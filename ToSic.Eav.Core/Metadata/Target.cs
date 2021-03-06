using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Metadata
{
    /// <inheritdoc />
    [PublicApi_Stable_ForUseInYourCode]
    public class Target : ITarget
    {
        /// <summary>
        /// Will return true if a target-type was assigned
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool IsMetadata => TargetType != Constants.NotMetadata;

        /// <summary>
        /// This is the AssignmentObjectTypeId - usually 1 (none), 2 (attribute), 4 (entity)
        /// </summary>
        public int TargetType { get; set; } = Constants.NotMetadata;

        /// <summary>
        /// The KeyNumber is null or the int of the key as stored in "Key"
        /// </summary>
        public int? KeyNumber { get; set; }

        /// <summary>
        /// The KeyGuid is null or the guid of the key as stored in "Key"
        /// </summary>
        public Guid? KeyGuid { get; set; }

        /// <summary>
        /// The KeyString is null or the string of the key as stored in "Key"
        /// </summary>
        public string KeyString { get; set; }

        /// <summary>
        /// Constructor for a new MetadataFor, which is empty.
        /// So it's not for anything, or the specs will be added afterwards.
        /// </summary>
        public Target() { }

        /// <summary>
        /// Constructor to copy an existing MetadataFor object. 
        /// </summary>
        /// <param name="originalToCopy"></param>
        public Target(ITarget originalToCopy)
        {
            TargetType = originalToCopy.TargetType;
            KeyString = originalToCopy.KeyString;
            KeyNumber = originalToCopy.KeyNumber;
            KeyGuid = originalToCopy.KeyGuid;

        }


    }
}