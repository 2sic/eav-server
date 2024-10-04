namespace ToSic.Eav.Apps.Internal.Work;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class PairTypeWithAttribute
{
    public IContentType Type { get; init; }
    public IContentTypeAttribute Attribute { get; init; }
}