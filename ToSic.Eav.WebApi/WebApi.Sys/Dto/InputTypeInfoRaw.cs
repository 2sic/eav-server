using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.WebApi.Sys.Dto;

/// <summary>
/// Raw entity wrapper for InputTypeInfo to enable automatic raw conversion.
/// This bridges the gap between InputTypeInfo (in ToSic.Eav.Apps) and IRawEntityAutoConvert (in ToSic.Eav.Data.Build).
/// </summary>
[ContentType(
    Name = "InputTypeInfo",
    Guid = "8c0b688e-c79a-4180-8123-5d1959f3a89f",
    Description = "Input type information",
    Scope = "System"
)]
public class InputTypeInfoRaw : IRawEntityAutoConvert
{
    public InputTypeInfoRaw(InputTypeInfo source)
    {
        Type = source.Type;
        Label = source.Label;
        Description = source.Description;
        DisableI18n = source.DisableI18n;
        UiAssets = source.UiAssets;
        UseAdam = source.UseAdam;
        IsObsolete = source.IsObsolete ?? false;
        ObsoleteMessage = source.ObsoleteMessage;
        IsRecommended = source.IsRecommended ?? false;
        IsDefault = source.IsDefault ?? false;
        Source = source.Source;
        ConfigTypes = source.ConfigTypes == null ? null : [source.ConfigTypes];
    }

    [ContentTypeField(IsTitle = true)]
    public string Type { get; init; }

    public string? Label { get; init; }

    public string? Description { get; init; }

    public bool DisableI18n { get; init; }

    public IDictionary<string, string> UiAssets { get; init; }

    public bool UseAdam { get; init; }

    public bool IsObsolete { get; init; }

    public string? ObsoleteMessage { get; init; }

    public bool IsRecommended { get; init; }

    public bool IsDefault { get; init; }

    public string? Source { get; init; }

    public string[]? ConfigTypes { get; init; }
}
