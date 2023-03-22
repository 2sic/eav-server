namespace ToSic.Eav.Data.ContentTypes
{
    public interface IDeferredContentTypeProvider
    {
        IContentType LazyTypeGenerator(int appId, string name, string nameId, IContentType fallback);
    }
}