
// 2025-01-05 2dm - doesn't seem used, turned of for now, remove ca. 2025-Q2

//namespace ToSic.Eav.Data.ContentTypes;

///// <summary>
///// Special helper class for delayed construction of ContentTypes so they can be immutable
///// </summary>
//[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
//public class DeferredContentTypeProvider : IDeferredContentTypeProvider
//{
//    public List<IContentType> Source { get; } = [];
//    public DeferredContentTypeProvider()
//    {
//    }

//    public IContentType LazyTypeGenerator(int appId, string name, string nameId, IContentType fallback)
//    {
//        var delayedType = new ContentTypeWrapper(() => Source.FirstOrDefault(t => t.Is(nameId)) ?? fallback);
//        return delayedType;
//    }
//}