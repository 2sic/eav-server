
// ReSharper disable InconsistentNaming

namespace ToSic.Eav.WebApi.Sys.ApiExplorer;

public class ApiActionParamDto
{
    public required string name { get; init; }
    public required string type { get; init; }
    public required object defaultValue { get; init; }
    public required bool isOptional { get; init; }
    public required bool isBody { get; init; }
}