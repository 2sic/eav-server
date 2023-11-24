using System.Collections.Generic;

// ReSharper disable InconsistentNaming

namespace ToSic.Eav.WebApi.ApiExplorer;

class ApiControllerDto
{
    public string controller { get; set; }
    public IEnumerable<ApiActionDto> actions { get; set; }
        
    public ApiSecurityDto security { get; set; }
}