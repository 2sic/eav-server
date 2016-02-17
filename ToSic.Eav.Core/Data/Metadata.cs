using System;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents a Dimension Assignment
    /// </summary>
    public class Metadata : IMetadata
    {

        /// <summary>
        /// Will return true if a target-type was assigned
        /// </summary>
        public bool HasMetadata
        {
            get { return TargetType != Constants.DefaultAssignmentObjectTypeId; }
        }

        /// <summary>
        /// This is the AssignmentObjectTypeId - usually 1 (none), 2 (attribute), 4 (entity)
        /// </summary>
        public int TargetType { get; set; }

        public string Key { get; set; }

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
    }
}