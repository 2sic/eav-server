namespace ToSic.Eav.Data.ContentTypes.Sys;

public interface IDeferredContentTypeProvider
{
    IContentType LazyTypeGenerator(int appId, string name, string nameId, IContentType fallback);
}