namespace ToSic.Eav.Data.Sys.ContentTypes;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IDeferredContentTypeProvider
{
    IContentType LazyTypeGenerator(int appId, string name, string nameId, IContentType fallback);
}