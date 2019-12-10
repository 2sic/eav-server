using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ToSic.Eav.ImportExport.Json.V1
{
    public class JsonEntity
    {
        public int Id;
        public int Version;
        public Guid Guid;
        public JsonType Type;
        public JsonAttributes Attributes;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public string Owner;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public JsonMetadataFor For;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public List<JsonEntity> Metadata;
    }
}
