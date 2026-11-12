namespace ToSic.Eav.Apps.Inspect.Sys.UiDto;

[ShowApiWhenReleased(ShowApiMode.Never)]
public struct ContentTypeUiInfo
{
    public string Name { get; init; }
    public string StaticName { get; init; }
    public bool IsHidden { get; init; }
    public IDictionary<string, object?>? Properties { get; init; }
    public string? Thumbnail { get; init; }
    public bool IsDefault { get; init; } // new, v13
}