using System.Collections.Generic;

namespace ToSic.Eav.Data.Builder
{
    public class ContentTypeBuilder
    {
        /// <summary>
        /// WIP - constructor shouldn't ever be called because of DI
        /// </summary>
        public ContentTypeBuilder() { }

        public const int DynTypeId = 1;
        // #RemoveContentTypeDescription #2974 - #remove ca. 2023 Q2 if all works
        //public const string DynTypeDefDescription = "Dynamic content type";

        public IContentType Transient(string typeName)
            => Transient(Constants.TransientAppId, typeName, typeName);

        public IContentType Transient(int appId, string typeName, string nameId, string scope = null)
            // #RemoveContentTypeDescription #2974 - #remove ca. Feb 2023 if all works
            => new ContentType(appId, typeName, nameId, DynTypeId, scope ?? Scopes.System/*, DynTypeDefDescription*/)
            {
                Attributes = new List<IContentTypeAttribute>(),
                IsDynamic = true
            };

    }
}
