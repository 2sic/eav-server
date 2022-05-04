using System;
using Newtonsoft.Json;

namespace ToSic.Eav.WebApi.Admin.Features
{
    public class FeaturesDto
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("msg")]
        public Msg Msg { get; set; }

    }
    public class Msg
    {
        [JsonProperty("features")]
        public string Features { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }
    }
}
