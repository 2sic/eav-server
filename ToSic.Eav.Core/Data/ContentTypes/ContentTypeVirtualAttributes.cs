namespace ToSic.Eav.Data;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
internal class ContentTypeVirtualAttributes(IDictionary<string, IContentTypeAttribute> vAttributes) : IDecorator<IContentType>
{
    public IDictionary<string, IContentTypeAttribute> VirtualAttributes => vAttributes;
}