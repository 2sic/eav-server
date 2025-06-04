namespace ToSic.Eav.WebApi.Sys.Dto;

public class AllFilesDto
{
    public IEnumerable<AllFileDto> Files = new List<AllFileDto>();
}

public class AllFileDto
{
    public string Path;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Shared;
}