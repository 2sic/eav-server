using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.WebApi.Sys.Security;

namespace ToSic.Eav.WebApi.Sys.Context;

public record ContextLanguageDto
{
    public required string Code { get; init; }

    public string NameId => Code.ToLowerInvariant();

    [ContentTypeTitle]
    public required string Culture { get; init; }

    public required bool IsEnabled { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required bool? IsAllowed { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required HasPermissionsDto? Permissions { get; init; }
}
