using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.WebApi.Sys.Security;

namespace ToSic.Eav.WebApi.Sys.Dto;

[ContentType(
    Name = "LanguageStatus",
    Guid = "c8676078-b904-4412-bf4e-aa83d48b63e7",
    Description = "Language enabled for an app",
    Scope = "System"
)]
public record LanguageStatusRaw : IRawEntityAutoConvert
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
