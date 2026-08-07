using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.WebApi.Sys.Dto;

[ContentType(
    Name = "InputTypeInfo",
    Guid = "8c0b688e-c79a-4180-8123-5d1959f3a89f",
    Description = "Input type information",
    Scope = "System"
)]
public record InputTypeInfoRaw : IRawEntityAutoConvert
{
    [ContentTypeField(IsTitle = true)]
    public required string type { get; init; }

    public required string? label { get; init; }
    public required string? description { get; init; }
    public required bool disableI18n { get; init; }
    public required IDictionary<string, string> uiAssets { get; init; }
    public required bool useAdam { get; init; }
    public required bool isObsolete { get; init; }
    public required string? obsoleteMessage { get; init; }
    public required bool isRecommended { get; init; }
    public required bool isDefault { get; init; }
    public required string? source { get; init; }
    public required string[]? configTypes { get; init; }
}
