namespace ToSic.Eav.Data.ContentTypes.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IDeferredContentTypeProvider
{
    IContentType LazyTypeGenerator(int appId, string name, string nameId, IContentType fallback);
}