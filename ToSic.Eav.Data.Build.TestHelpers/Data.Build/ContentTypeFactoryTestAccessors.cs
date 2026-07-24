using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.Data.Sys.Entities;

namespace ToSic.Eav.Data.Build;

public static class ContentTypeFactoryTestAccessors
{
    extension(ContentTypesFromCodeManager ctDefManager)
    {
        public IContentType CreateTac(Type t)
            => ctDefManager.Get(t);

        public bool IsConfiguredTac(Type t)
            => ctDefManager.IsConfigured(t);

        public IContentType CreateTac<T>()
            => ctDefManager.Get<T>();

        public ContentTypeVirtualAttributes GetVirtualAttribDecorator(Type t)
            => ctDefManager.CreateTac(t).GetDecorator<ContentTypeVirtualAttributes>()!;

    }
}
