namespace ToSic.Eav.Apps.Inspect.Sys.UiDto;

[ShowApiWhenReleased(ShowApiMode.Never)]
public struct TemplateUiInfo
{
    public int TemplateId { get; init; }
    public string Name { get; init; }
    public string ContentTypeStaticName { get; init; }
    public bool IsHidden { get; init; }
    public string? Thumbnail { get; init; }
    public bool IsDefault { get; init; } // new, v13
}