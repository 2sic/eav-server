using ToSic.Eav.WebApi.Sys.Security;

namespace ToSic.Eav.WebApi.Sys.Dto;

public class SiteLanguageDto
{
    public required string Code { get; init; }

    public string? NameId => Code?.ToLowerInvariant();

    public required string Culture { get; init; }

    public required bool IsEnabled { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsAllowed { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public HasPermissionsDto? Permissions { get; init; }
}