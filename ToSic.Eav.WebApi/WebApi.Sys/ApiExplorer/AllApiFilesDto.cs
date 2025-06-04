using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.ApiExplorer;

public class AllApiFilesDto
{
    public IEnumerable<AllApiFileDto> Files = [];
}

public class AllApiFileDto : AllFileDto
{
    public string EndpointPath;
    public string Edition;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsCompiled;
}