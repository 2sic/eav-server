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
    [ContentTypeTitle]
    public required string Type { get; init; }

    public required string? Label { get; init; }
    public required string? Description { get; init; }
    public required bool DisableI18n { get; init; }
    public required IDictionary<string, string> UiAssets { get; init; }
    public required bool UseAdam { get; init; }
    public required bool IsObsolete { get; init; }
    public required string? ObsoleteMessage { get; init; }
    public required bool IsRecommended { get; init; }
    public required bool IsDefault { get; init; }
    public required string? Source { get; init; }
    public required string[]? ConfigTypes { get; init; }
}
