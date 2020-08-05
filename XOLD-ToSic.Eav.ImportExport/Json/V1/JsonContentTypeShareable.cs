using Newtonsoft.Json;

namespace ToSic.Eav.ImportExport.Json.V1
{
    public class JsonContentTypeShareable
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)] public bool AlwaysShare;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)] public int ParentZoneId;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)] public int ParentAppId;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore)]
        public int? ParentId;
    }
}
