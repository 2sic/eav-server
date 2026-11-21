using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.Ui.Edit.Sys.Load.Links;

public class LinkInfoDto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AdamItemDto? Adam { get; init; }
    public required string Value { get; init; }
}