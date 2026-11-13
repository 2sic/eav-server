using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.Ui.Context.Sys.Dto;

public record ContentBlockDto : IIdentifierDto
{
    public int Id { get; init; }
    public Guid Guid { get; init; }

    public required IEnumerable<InstanceDto>? Modules { get; init; }
}