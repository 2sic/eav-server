using System;
using Newtonsoft.Json;

namespace ToSic.Eav.ImportExport.Json.V1
{
    public class JsonMetadataFor
    {
        public string Target;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public string String;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Guid? Guid;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public int? Number;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public bool? Singleton;
    }
}
