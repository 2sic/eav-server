using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.WebApi.Sys.Security;

namespace ToSic.Eav.WebApi.Sys.Dto;

[ContentType(
    Name = "AppLanguage",
    Guid = "c8676078-b904-4412-bf4e-aa83d48b63e7",
    Description = "Language enabled for an app",
    Scope = "System"
)]
public class AppLanguageRaw : IRawEntityAutoConvert
{
    public required string Code { get; init; }

    public string? NameId => Code?.ToLowerInvariant();

    [ContentTypeField(IsTitle = true)]
    public required string Culture { get; init; }

    public required bool IsEnabled { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsAllowed { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public HasPermissionsDto? Permissions { get; init; }
}
