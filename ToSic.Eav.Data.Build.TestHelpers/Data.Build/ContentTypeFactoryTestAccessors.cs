namespace ToSic.Eav.Data.Build;

public static class ContentTypeFactoryTestAccessors
{
    extension(CodeContentTypesManager ctDefFactory)
    {
        public IContentType CreateTac(Type t)
            => ctDefFactory.Get(t);

        public bool IsConfiguredTac(Type t)
            => ctDefFactory.IsConfigured(t);

        public IContentType CreateTac<T>()
            => ctDefFactory.Get<T>();
    }
}
