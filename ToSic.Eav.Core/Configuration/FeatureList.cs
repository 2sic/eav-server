using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ToSic.Eav.Configuration
{
    public class FeatureList
    {
        [JsonProperty("features")]
        public List<Feature> Features;


        public FeatureList(IEnumerable<Feature> prefill = null)
        {
            Features = prefill?.ToList() ?? new List<Feature>();
        }
    }

    public class FeatureListWithFingerprint : FeatureList
    {
        [JsonProperty("fingerprint")]
        public string Fingerprint;
    }
}
