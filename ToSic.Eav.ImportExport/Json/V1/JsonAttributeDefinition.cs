using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.Json.V1
{
    public class JsonAttributeDefinition
    {
        public string Name;
        public string Type;
        public string InputType;    // added 2019-09-02 for 2sxc 10.03 to enhance UI handling
        public bool IsTitle;

        /// <summary>
        /// Metadata Entities for this Attribute such as description, dropdown values etc.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<JsonEntity> Metadata;

        /// <summary>
        /// WIP in 16.08+
        /// #SharedFieldDefinition
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? Guid { get; set; }

        /// <summary>
        /// WIP in 16.08+
        /// #SharedFieldDefinition
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public JsonAttributeSysSettings SysSettings { get; set; }
    }
}
