using System;
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

        public ContentTypeAttributeSysSettings(Guid? sourceGuid, bool inheritName, bool inheritMetadata, AttributeShareLevel shareLevel)
        {
            SourceGuid = sourceGuid;
            InheritName = inheritName;
            InheritMetadata = inheritMetadata;
            ShareLevel = shareLevel;
        }

        public Guid? SourceGuid { get; }

        public bool InheritName { get; }

        public bool InheritMetadata { get; }

        /// <summary>
        /// Probably mark this field as allowing share in the same app
        /// </summary>
        public AttributeShareLevel ShareLevel { get; }
    }
    
    public enum AttributeShareLevel
    {
        None = 0,
        SameApp = 1,
        // SameSite = 2,
    }
}
