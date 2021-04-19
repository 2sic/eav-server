using System;
using Newtonsoft.Json;

namespace ToSic.Eav.Conversion
{
    public class RelationshipReference
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Id;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Title;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Guid? Guid;
    }
}
