using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Configuration
{
    [PrivateApi("no good reason to publish this")]
    public class FeatureList
    {
        [JsonProperty("features")]
        public List<Feature> Features;


        public FeatureList(IEnumerable<Feature> prefill = null)
        {
            Features = prefill?.ToList() ?? new List<Feature>();
        }
    }

    [PrivateApi("no good reason to publish this")]
    public class FeatureListStored
    {
        [JsonProperty("features")]
        public List<FeatureConfig> Features = new List<FeatureConfig>();

        [JsonProperty("fingerprint")]
        public string Fingerprint;
    }
}
