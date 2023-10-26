using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ToSic.Eav.ImportExport.Json.V1
{
    public class JsonContentType: IJsonWithAssets
    {
        /// <remarks>V 1.0</remarks>
        [JsonPropertyOrder(1)]
        public string Id;

        /// <remarks>V 1.0</remarks>
        [JsonPropertyOrder(2)]
        public string Name;

        /// <remarks>V 1.0</remarks>
        [JsonPropertyOrder(3)]
        public string Scope;

        // TODO: Don't just remove, it's possible that we're using it in the admin-UI...
        // Review w/2dm before removing this code
        // #RemoveContentTypeDescription #2974 - #remove ca. Feb 2023 if all works
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
