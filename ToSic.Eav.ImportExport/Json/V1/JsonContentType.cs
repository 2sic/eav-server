using System.Collections.Generic;
using Newtonsoft.Json;

namespace ToSic.Eav.ImportExport.Json.V1
{
    public class JsonContentType
    {
        public string Id;
        public string Name;
        public string Scope;
        public string Description;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public List<JsonAttributeDefinition> Attributes;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public JsonContentTypeShareable Sharing;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public List<JsonEntity> Metadata;
    }
}
