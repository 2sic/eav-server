using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using ToSic.Eav.Data;
using ToSic.Eav.Plumbing;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.Json.V1
{
    /// <summary>
    /// WIP 16.08+
    /// #SharedFieldDefinition
    /// </summary>
    public class JsonAttributeSysSettings
    {
        /// <summary>
        /// Old, remove soon
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Guid? SourceGuid { get; set; }

        /// <summary>
        /// New
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Guid? Inherit { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool InheritName { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool InheritMetadata { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string InheritMetadataOf { get; set; }

        /// <summary>
        /// Mark this Attribute that it shares itself / its properties
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool Share { get; set; }

        // future
        //public bool ShareHidden { get; set; }

        public static JsonAttributeSysSettings FromSysSettings(ContentTypeAttributeSysSettings sysSettings) =>
            sysSettings == null 
                ? null 
                : new JsonAttributeSysSettings
                {
                    Share = sysSettings.Share,
                    Inherit = sysSettings.Inherit,
                    InheritMetadataOf = sysSettings.InheritMetadataOf?.Any() != true
                        ? null
                        : string.Join(",", sysSettings.InheritMetadataOf.Select(p =>
                            p.Key + (p.Value.IsEmptyOrWs() || p.Value == "*" ? "" : p.Value)))
                };


        public ContentTypeAttributeSysSettings ToSysSettings() =>
            new ContentTypeAttributeSysSettings(Share, inherit: Inherit ?? SourceGuid, inheritName: InheritName,
                inheritMetadata: InheritMetadata, ConvertInheritMetadataStringToDicOrNull());

        private Dictionary<Guid, string> ConvertInheritMetadataStringToDicOrNull()
        {
            var inheritDic = InheritMetadataOf?.Trim().Split(',')
                .Select(s =>
                {
                    var parts = s.Split('/');
                    return parts.Length > 0
                        ? new
                        {
                            Key = Guid.TryParse(parts[0], out var key) ? key : Guid.Empty,
                            Value = parts.Length > 1 ? parts[1] : null
                        }
                        : null;
                })
                .Where(p => p != null && p.Key != Guid.Empty)
                .ToDictionary(p => p.Key, p => p.Value);
            return inheritDic;
        }
    }
}
