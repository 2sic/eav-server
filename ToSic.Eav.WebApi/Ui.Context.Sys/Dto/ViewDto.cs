using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.Ui.Context.Sys.Dto;

public record ViewDto: IIdentifierDto
{
    public int Id { get; init; }
    public Guid Guid { get; init; }

    public required string Name { get; init; }
    public string? Path { get; init; }
    public required IEnumerable<ContentBlockDto> Blocks { get; init; }

}