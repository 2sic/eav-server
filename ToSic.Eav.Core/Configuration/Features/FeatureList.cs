using System.Collections.Generic;
using Newtonsoft.Json;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Configuration
{
    [PrivateApi("no good reason to publish this")]
    public class FeatureListStored
    {
        [JsonProperty("features")]
        public List<FeatureConfig> Features = new List<FeatureConfig>();

        [JsonProperty("fingerprint")]
        public string Fingerprint;
    }
}
