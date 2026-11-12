using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.Ui.Context.Sys.Dto;

public class ContentBlockDto : IdentifierDto
{
    public required IEnumerable<InstanceDto>? Modules { get; init; }
}