using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.Context;

public class ContentBlockDto : IdentifierDto
{
    public required IEnumerable<InstanceDto>? Modules { get; init; }
}