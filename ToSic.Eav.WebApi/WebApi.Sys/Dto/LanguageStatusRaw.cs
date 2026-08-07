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
    public required string code { get; init; }

    public string nameId => code.ToLowerInvariant();

    [ContentTypeField(IsTitle = true)]
    public required string culture { get; init; }

    public required bool isEnabled { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required bool? isAllowed { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required HasPermissionsDto? permissions { get; init; }
}
