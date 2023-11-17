using System.Collections.Generic;
using System.Text.Json.Serialization;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.SysData
{
    [PrivateApi("no good reason to publish this")]
    public class FeatureStatesPersisted
    {
        [JsonPropertyName("features")]
        public List<FeatureStatePersisted> Features = new List<FeatureStatePersisted>();

        [JsonPropertyName("fingerprint")]
        public string Fingerprint;
    }
}
