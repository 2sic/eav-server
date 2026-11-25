namespace ToSic.Eav.Data.ContentTypes.Fields.Sys;

[ContentType(
    Name = "InputTypeInfo",
    Guid = "ede9f818-c4ad-46a3-b738-55c0d4a43218",
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
    [ContentTypeField(Type = ValueTypes.Object)]
    public required IDictionary<string, string> UiAssets { get; init; }
    public required bool UseAdam { get; init; }
    public required bool IsObsolete { get; init; }
    public required string? ObsoleteMessage { get; init; }
    public required bool IsRecommended { get; init; }
    public required bool IsDefault { get; init; }
    public required string? Source { get; init; }
    [ContentTypeField(Type = ValueTypes.Object)]
    public required string[]? ConfigTypes { get; init; }
}
