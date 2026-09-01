namespace ToSic.Eav.WebApi.Sys.Dto;

public record AllFilesDto
{
    public required IEnumerable<AllFileDto> Files = new List<AllFileDto>();
}

public record AllFileDto
{
    public required string Path { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Shared { get; init; }
}