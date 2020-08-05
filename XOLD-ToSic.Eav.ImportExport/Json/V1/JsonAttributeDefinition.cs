using System.Collections.Generic;
using Newtonsoft.Json;

namespace ToSic.Eav.ImportExport.Json.V1
{
    public class JsonAttributeDefinition
    {
        public string Name;
        public string Type;
        public string InputType;    // added 2019-09-02 for 2sxc 10.03 to enhance UI handling
        public bool IsTitle;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public List<JsonEntity> Metadata;
    }
}
