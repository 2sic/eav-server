using ToSic.Eav.Data;

namespace ToSic.Eav.Apps.Work;

public class PairTypeWithAttribute
{
    public IContentType Type { get; set; }
    public IContentTypeAttribute Attribute { get; set; }
}