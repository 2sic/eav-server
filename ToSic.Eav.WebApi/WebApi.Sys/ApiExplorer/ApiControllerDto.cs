

// ReSharper disable InconsistentNaming

namespace ToSic.Eav.WebApi.Sys.ApiExplorer;

class ApiControllerDto
{
    public required string controller { get; init; }
    public required IEnumerable<ApiActionDto> actions { get; init; }
        
    public required ApiSecurityDto security { get; init; }
}