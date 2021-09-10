using System;
using Newtonsoft.Json;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.ImportExport.Convert.EntityToDictionaryLight
{
    [PrivateApi]
    public class RelationshipReferenceDto
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Id;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Title;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Guid? Guid;
    }
}
