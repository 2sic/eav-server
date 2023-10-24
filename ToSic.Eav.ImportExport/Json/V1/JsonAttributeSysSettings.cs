using System;
using System.Text.Json.Serialization;
using ToSic.Eav.Data;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.Json.V1
{
    /// <summary>
    /// WIP 16.08+
    /// #SharedFieldDefinition
    /// </summary>
    public class JsonAttributeSysSettings
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Guid? SourceGuid { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool InheritName { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool InheritMetadata { get; set; }

        /// <summary>
        /// Probably mark this field as allowing share in the same app
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int ShareLevel { get; set; }

        public ContentTypeAttributeSysSettings ToSysSettings() 
            => new ContentTypeAttributeSysSettings(sourceGuid: SourceGuid, inheritName: InheritName, inheritMetadata: InheritMetadata, shareLevel: (AttributeShareLevel)ShareLevel);

        public static JsonAttributeSysSettings FromSysSettings(ContentTypeAttributeSysSettings sysSettings) =>
            sysSettings == null
                ? null
                : new JsonAttributeSysSettings
                {
                    SourceGuid = sysSettings.SourceGuid,
                    InheritName = sysSettings.InheritName,
                    ShareLevel = (int)sysSettings.ShareLevel,
                    InheritMetadata = sysSettings.InheritMetadata,
                };
    }
}
