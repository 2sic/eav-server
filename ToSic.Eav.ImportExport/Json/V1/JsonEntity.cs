using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ToSic.Eav.ImportExport.Json.V1
{
    public class JsonEntity: IJsonWithAssets
    {
        /// <remarks>V 1.0</remarks>
        public int Id;

        /// <remarks>V 1.0</remarks>
        public int Version;

        /// <remarks>V 1.0</remarks>
        public Guid Guid;

        /// <remarks>V 1.0</remarks>
        public JsonType Type;

        /// <remarks>V 1.0</remarks>
        public JsonAttributes Attributes;

        /// <remarks>V 1.0</remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] public string Owner;

        /// <remarks>V 1.0</remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] public JsonMetadataFor For;

        /// <remarks>V 1.0</remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] public List<JsonEntity> Metadata;

        /// <remarks>V 1.1</remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] public List<JsonAsset> Assets { get; set; }
    }
}
