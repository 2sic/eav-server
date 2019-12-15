using Newtonsoft.Json;

namespace ToSic.Eav.ImportExport.Json.V1
{
    public class JsonFormat
    {
        public JsonHeader _ = new JsonHeader();
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public JsonEntity Entity;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public JsonContentType ContentType;
    }
}
