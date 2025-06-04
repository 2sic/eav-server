using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.Context;

public class ViewDto : IdentifierDto
{
    public string Name;
    public string Path;
    public IEnumerable<ContentBlockDto> Blocks;

}