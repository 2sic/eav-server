using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ToSic.Eav.ImportExport.Json.Format
{

    internal class JsonHeader { public int V = 1; }

    internal class JsonMetadataFor
    {
        public string Target;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public string String;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Guid? Guid;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public int? Number;
    }

    internal class JsonType { public string Name, Id; }

    internal class JsonFormat
    {
        public JsonHeader _ = new JsonHeader();
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public JsonEntity Entity;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public JsonContentType ContentType;
    }

    internal class JsonEntity
    {
        public int Id;
        public int Version;
        public Guid Guid;
        public JsonType Type;
        public JsonAttributes Attributes;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public string Owner;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public JsonMetadataFor For;
    }

    internal class JsonAttributes
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, Dictionary<string, string>> String;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, Dictionary<string, string>> Hyperlink;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, Dictionary<string, string>> Custom;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, Dictionary<string, List<Guid?>>> Entity;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, Dictionary<string, decimal?>> Number;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, Dictionary<string, DateTime?>> DateTime;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, Dictionary<string, bool?>> Boolean;
    }

    internal class JsonContentType
    {
        public string Id;
        public string Name;
        public string Scope;
        public string Description;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public List<JsonAttributeDefinition> Attributes;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public JsonContentTypeShareable Sharing;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public List<JsonEntity> Metadata;
    }

    internal class JsonContentTypeShareable
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)] public bool AlwaysShare;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)] public int ParentZoneId;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)] public int ParentAppId;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, 
            NullValueHandling = NullValueHandling.Ignore)] public int? ParentId;
    }

    internal class JsonAttributeDefinition
    {
        public string Name;
        public string Type;
        public bool IsTitle;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public List<JsonEntity> Metadata;
    }

}
