using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.Context;

public class ContentBlockDto : IdentifierDto
{
    public IEnumerable<InstanceDto> Modules;
}