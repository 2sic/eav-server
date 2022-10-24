using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ToSic.Eav.ImportExport.Json.V1
{
    public class JsonContentType: IJsonWithAssets
    {
        /// <remarks>V 1.0</remarks>
        public string Id;

        /// <remarks>V 1.0</remarks>
        public string Name;

        /// <remarks>V 1.0</remarks>
        public string Scope;

        /// <remarks>V 1.0</remarks>
        public string Description;

        /// <remarks>V 1.0</remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<JsonAttributeDefinition> Attributes;

        /// <remarks>V 1.0</remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public JsonContentTypeShareable Sharing;

        /// <remarks>V 1.0</remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<JsonEntity> Metadata;

        /// <remarks>V 1.1</remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<JsonAsset> Assets { get; set; }
    }
}
