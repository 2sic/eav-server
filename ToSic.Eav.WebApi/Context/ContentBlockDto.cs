using System.Collections.Generic;
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.Context
{
    public class ContentBlockDto : IdentifierDto
    {
        public IEnumerable<InstanceDto> Modules;
    }
}
