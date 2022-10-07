using System.Collections.Generic;
using System.Text.Json.Serialization;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Configuration
{
    [PrivateApi("no good reason to publish this")]
    public class FeatureListStored
    {
        [JsonPropertyName("features")]
        public List<FeatureConfig> Features = new List<FeatureConfig>();

        [JsonPropertyName("fingerprint")]
        public string Fingerprint;
    }
}
