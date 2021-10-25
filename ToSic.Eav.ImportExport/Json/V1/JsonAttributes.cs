using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ToSic.Eav.ImportExport.Json.V1
{
    public class JsonAttributes
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, Dictionary<string, string>> String;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, Dictionary<string, string>> Hyperlink;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, Dictionary<string, string>> Custom;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, Dictionary<string, string>> Json;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, Dictionary<string, List<Guid?>>> Entity;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, Dictionary<string, decimal?>> Number;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, Dictionary<string, DateTime?>> DateTime;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, Dictionary<string, bool?>> Boolean;
    }
}
