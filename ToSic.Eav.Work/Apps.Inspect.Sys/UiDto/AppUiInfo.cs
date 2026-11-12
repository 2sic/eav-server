namespace ToSic.Eav.Apps.Inspect.Sys.UiDto;

[ShowApiWhenReleased(ShowApiMode.Never)]
public struct AppUiInfo
{
    public string Name { get; init; }
    public int AppId { get; init; }
    public bool SupportsAjaxReload { get; init; }
    public string? Thumbnail { get; init; }
    public string Version { get; init; }
}