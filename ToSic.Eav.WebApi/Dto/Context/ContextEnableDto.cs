using System.Text.Json.Serialization;
using static System.Text.Json.Serialization.JsonIgnoreCondition;

namespace ToSic.Eav.WebApi.Dto;

public class ContextEnableDto
{
    [JsonIgnore(Condition = WhenWritingNull)] public bool? AppPermissions { get; set; }
    [JsonIgnore(Condition = WhenWritingNull)] public bool? CodeEditor { get; set; }
    [JsonIgnore(Condition = WhenWritingNull)] public bool? Query { get; set; }

    [JsonIgnore(Condition = WhenWritingNull)] public bool? FormulaSave { get; set; }
    [JsonIgnore(Condition = WhenWritingNull)] public bool? OverrideEditRestrictions { get; set; }
    [JsonIgnore(Condition = WhenWritingNull)] public bool? DebugMode { get; set; }
}