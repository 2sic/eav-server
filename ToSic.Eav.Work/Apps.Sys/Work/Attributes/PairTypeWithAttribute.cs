namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class PairTypeWithAttribute
{
    public required IContentType Type { get; init; }
    public required IContentTypeAttribute Attribute { get; init; }
}