namespace ToSic.Eav.Data.Build;

public static class ContentTypeFactoryTestAccessors
{
    extension(ContentTypeFactory factory)
    {
        public IContentType CreateTac(Type t)
            => factory.Create(t);

        public IContentType CreateTac<T>()
            => factory.Create<T>();
    }
}
