using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ToSic.Eav.ImportExport.Json.V1
{
    public class JsonAttributes
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public Dictionary<string, Dictionary<string, string>> String;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public Dictionary<string, Dictionary<string, string>> Hyperlink;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public Dictionary<string, Dictionary<string, string>> Custom;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public Dictionary<string, Dictionary<string, string>> Json;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public Dictionary<string, Dictionary<string, List<Guid?>>> Entity;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public Dictionary<string, Dictionary<string, decimal?>> Number;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public Dictionary<string, Dictionary<string, DateTime?>> DateTime;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public Dictionary<string, Dictionary<string, bool?>> Boolean;
    }
}
