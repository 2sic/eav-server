namespace ToSic.Eav.WebApi.Sys.Dto;

public class LinkInfoDto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AdamItemDto? Adam { get; init; }
    public required string Value { get; init; }
}