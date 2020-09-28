using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ToSic.Eav.ImportExport.Json.V1
{
    public class JsonEntity: IJsonWithAssets
    {
        /// <remarks>V 1.0</remarks>
        public int Id;

        /// <remarks>V 1.0</remarks>
        public int Version;

        /// <remarks>V 1.0</remarks>
        public Guid Guid;

        /// <remarks>V 1.0</remarks>
        public JsonType Type;

        /// <remarks>V 1.0</remarks>
        public JsonAttributes Attributes;

        /// <remarks>V 1.0</remarks>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public string Owner;

        /// <remarks>V 1.0</remarks>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public JsonMetadataFor For;

        /// <remarks>V 1.0</remarks>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public List<JsonEntity> Metadata;

        /// <remarks>V 1.1</remarks>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public List<JsonAsset> Assets { get; set; }
    }
}
