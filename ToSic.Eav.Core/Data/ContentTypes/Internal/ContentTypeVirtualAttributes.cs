namespace ToSic.Eav.Data.Internal;

/// <summary>
/// Special Decorator for Content-Types.
/// If added, it can contain descriptions for system attributes such as "ID" which would otherwise
/// just have a default description.
/// </summary>
/// <param name="vAttributes"></param>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ContentTypeVirtualAttributes(IDictionary<string, IContentTypeAttribute> vAttributes) : IDecorator<IContentType>
{
    public IDictionary<string, IContentTypeAttribute> VirtualAttributes => vAttributes;
}