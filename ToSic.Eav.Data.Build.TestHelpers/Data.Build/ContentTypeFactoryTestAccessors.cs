using ToSic.Eav.Data.ContentTypes.Sys;
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

        public ContentTypeBuiltInAttributesDecorator GetVirtualAttribDecoratorOf(Type t)
            => ctDefManager.CreateTac(t).GetDecorator<ContentTypeBuiltInAttributesDecorator>()!;

    }
}
