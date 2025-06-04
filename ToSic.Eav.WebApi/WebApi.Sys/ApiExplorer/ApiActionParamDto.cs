
// ReSharper disable InconsistentNaming

namespace ToSic.Eav.WebApi.Sys.ApiExplorer;

public class ApiActionParamDto
{
    public string name { get; set; }
    public string type { get; set; }
    public object defaultValue { get; set; }
    public bool isOptional { get; set; }
    public bool isBody { get; set; }
}