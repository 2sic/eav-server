using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ToSic.Eav.ImportExport.Json.Format
{

    public class JsonHeader { public int V = 1; }

    public class JsonMetadataFor
    {
        public string Target;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public string String;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Guid? Guid;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public int? Number;
    }

    public class JsonType { public string Name, Id; }

    public class JsonFormat
    {
        public JsonHeader _ = new JsonHeader();
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public JsonEntity Entity;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public JsonContentType ContentType;
    }

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

    public class JsonAttributes
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, Dictionary<string, string>> String;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, Dictionary<string, string>> Hyperlink;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, Dictionary<string, string>> Custom;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, Dictionary<string, List<Guid?>>> Entity;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, Dictionary<string, decimal?>> Number;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, Dictionary<string, DateTime?>> DateTime;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, Dictionary<string, bool?>> Boolean;
    }

    public class JsonContentType
    {
        public string Id;
        public string Name;
        public string Scope;
        public string Description;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public List<JsonAttributeDefinition> Attributes;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public JsonContentTypeShareable Sharing;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public List<JsonEntity> Metadata;
    }

    public class JsonContentTypeShareable
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)] public bool AlwaysShare;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)] public int ParentZoneId;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)] public int ParentAppId;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, 
            NullValueHandling = NullValueHandling.Ignore)] public int? ParentId;
    }

    public class JsonAttributeDefinition
    {
        public string Name;
        public string Type;
        public string InputType;    // added 2019-09-02 for 2sxc 10.03 to enhance UI handling
        public bool IsTitle;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public List<JsonEntity> Metadata;
    }

}
