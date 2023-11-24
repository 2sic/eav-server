using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using static System.Text.Json.Serialization.JsonIgnoreCondition;

namespace ToSic.Eav.ImportExport.Json.V1;

public class JsonAttributes
{
    [JsonIgnore(Condition = WhenWritingNull)] public Dictionary<string, Dictionary<string, string>> String;

    [JsonIgnore(Condition = WhenWritingNull)] public Dictionary<string, Dictionary<string, string>> Hyperlink;

    [JsonIgnore(Condition = WhenWritingNull)] public Dictionary<string, Dictionary<string, string>> Custom;

    [JsonIgnore(Condition = WhenWritingNull)] public Dictionary<string, Dictionary<string, string>> Json;

    [JsonIgnore(Condition = WhenWritingNull)] public Dictionary<string, Dictionary<string, List<Guid?>>> Entity;

    [JsonIgnore(Condition = WhenWritingNull)] public Dictionary<string, Dictionary<string, decimal?>> Number;

    [JsonIgnore(Condition = WhenWritingNull)] public Dictionary<string, Dictionary<string, DateTime?>> DateTime;

    [JsonIgnore(Condition = WhenWritingNull)] public Dictionary<string, Dictionary<string, bool?>> Boolean;
}