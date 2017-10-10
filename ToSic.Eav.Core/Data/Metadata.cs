using System;
using System.Collections.Generic;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    public class Metadata : IMetadata, IHasMetadata
    {
        /// <summary>
        /// Will return true if a target-type was assigned
        /// </summary>
        public bool IsMetadata => TargetType != Constants.NotMetadata;

        /// <summary>
        /// This is the AssignmentObjectTypeId - usually 1 (none), 2 (attribute), 4 (entity)
        /// </summary>
        public int TargetType { get; set; } = Constants.NotMetadata;

        //public string Key { get; set; }

        /// <summary>
        /// The Keynumber is null or the int of the key as stored in "Key"
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

        public Metadata CloneIsMetadata() => new Metadata
        {
            TargetType = TargetType,
            KeyString = KeyString,
            KeyNumber = KeyNumber,
            KeyGuid = KeyGuid
        };


        #region HasMetadata
        public bool HasMetadata { get; set; } = false;


        public List<IEntity> MetadataItems { get; set; } = null;
        #endregion
    }
}