using System.Collections.Generic;
using Newtonsoft.Json;

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
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public List<JsonAttributeDefinition> Attributes;

        /// <remarks>V 1.0</remarks>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public JsonContentTypeShareable Sharing;

        /// <remarks>V 1.0</remarks>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public List<JsonEntity> Metadata;

        /// <remarks>V 1.1</remarks>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public List<JsonAsset> Assets { get; set; }
    }
}
