namespace ToSic.Eav.Apps.Internal.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class PairTypeWithAttribute
{
    public IContentType Type { get; init; }
    public IContentTypeAttribute Attribute { get; init; }
}