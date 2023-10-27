using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// todo
    /// #SharedFieldDefinition
    /// </summary>
    [PrivateApi]
    public class ContentTypeAttributeSysSettings
	{
        public ContentTypeAttributeSysSettings() { }

        public ContentTypeAttributeSysSettings(bool share, Guid? inherit, bool inheritName, bool inheritMetadata, Dictionary<Guid, string> inheritMetadataOf)
        {
            Share = share;
            Inherit = inherit;
            InheritNameOfPrimary = inheritName;
            InheritMetadataOfPrimary = inheritMetadata;
            InheritMetadataOf = inheritMetadataOf;
        }

        #region Sharing / Source

        /// <summary>
        /// Mark this Attribute that it shares itself / its properties
        /// </summary>
        public bool Share { get; }

        #endregion

        /// <summary>
        /// Inherits-reference, ATM no purpose yet
        /// </summary>
        public Guid? Inherit { get; }

        /// <summary>
        /// Stored value - should usually NOT be used; ATM no purpose yet
        /// </summary>
        public bool InheritNameOfPrimary { get; }

        /// <summary>
        /// Stored value - should usually NOT be used, instead use InheritMetadata
        /// </summary>
        public bool InheritMetadataOfPrimary { get; }

        public Dictionary<Guid, string> InheritMetadataOf { get; set; }

        public bool InheritMetadata => InheritMetadataOf?.Any() == true || (Inherit != null && InheritMetadataOfPrimary);

        public Guid? InheritMetadataMainGuid => InheritMetadataOf?.Any() == true 
            ? InheritMetadataOf.FirstOrDefault().Key 
            : InheritMetadataOfPrimary ? Inherit : null;

    }
    
}
