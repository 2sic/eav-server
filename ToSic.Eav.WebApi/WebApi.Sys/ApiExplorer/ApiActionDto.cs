// ReSharper disable InconsistentNaming

namespace ToSic.Eav.WebApi.Sys.ApiExplorer;

public class ApiActionDto
{
    public required string name { get; init; }
    public required IEnumerable<string> verbs { get; init; }
    public required IEnumerable<ApiActionParamDto> parameters { get; init; }
    public required ApiSecurityDto security { get; init; }
    public required ApiSecurityDto mergedSecurity { get; init; }
    public required string returns { get; init; }
}