using System.Collections.Generic;

namespace ToSic.Eav.Data.Builder
{
    public static class ContentTypeBuilder
    {
        public const int DynTypeId = 1;
        public const string DynTypeDefDescription = "Dynamic content type";

        public static IContentType Fake(string typeName)
            => DynamicContentType(Constants.TransientAppId, typeName, typeName);

        public static IContentType DynamicContentType(int appId, string typeName, string typeIdentifier, string scope = null)
            => new ContentType(appId, typeName, typeIdentifier, DynTypeId, scope ?? Scopes.System, DynTypeDefDescription)
            {
                Attributes = new List<IContentTypeAttribute>(),
                IsDynamic = true
            };

    }
}
