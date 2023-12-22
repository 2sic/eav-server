using System.Collections.Generic;
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.Context;

public class ViewDto : IdentifierDto
{
    public string Name;
    public string Path;
    public IEnumerable<ContentBlockDto> Blocks;

}