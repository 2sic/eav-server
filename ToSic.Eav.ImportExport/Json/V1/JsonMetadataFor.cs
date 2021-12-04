using System;
using Newtonsoft.Json;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.ImportExport.Json.V1
{
    public class JsonMetadataFor
    {
        public string Target;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public string String;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Guid? Guid;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public int? Number;
        
        [PrivateApi("only used internally for now, name may change")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public bool? Singleton;


        [PrivateApi("only used internally for now")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public string Title;
    }
}
