namespace ToSic.Eav.WebApi.Sys.Dto;

public class AllFilesDto
{
    public required IEnumerable<AllFileDto> Files = new List<AllFileDto>();
}

public class AllFileDto
{
    public required string Path { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Shared { get; init; }
}