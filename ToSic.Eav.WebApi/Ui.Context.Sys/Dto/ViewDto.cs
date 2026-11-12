using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.Ui.Context.Sys.Dto;

public class ViewDto : IdentifierDto
{
    public required string Name { get; init; }
    public string? Path { get; init; }
    public required IEnumerable<ContentBlockDto> Blocks { get; init; }

}