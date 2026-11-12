using static System.Text.Json.Serialization.JsonIgnoreCondition;

namespace ToSic.Eav.Ui.Context.Sys.Dto;

public record ContextEnableDto
{
    [JsonIgnore(Condition = WhenWritingNull)]
    public bool? AppPermissions { get; init; }

    [JsonIgnore(Condition = WhenWritingNull)]
    public bool? CodeEditor { get; init; }

    [JsonIgnore(Condition = WhenWritingNull)]
    public bool? Query { get; init; }


    [JsonIgnore(Condition = WhenWritingNull)]
    public bool? FormulaSave { get; init; }

    [JsonIgnore(Condition = WhenWritingNull)]
    public bool? OverrideEditRestrictions { get; init; }

    [JsonIgnore(Condition = WhenWritingNull)]
    public bool? DebugMode { get; init; }
}