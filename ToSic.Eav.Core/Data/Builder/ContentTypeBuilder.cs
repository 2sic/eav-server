using System.Collections.Generic;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Data.Builder
{
    public static class ContentTypeBuilder
    {
        /// <summary>
        /// Shortcut go get a new AttributeSet with Scope=System and Name=StaticName
        /// </summary>
        public static ContentType SystemAttributeSet(int appId, string staticName, string description,
            List<IContentTypeAttribute> attributes, bool alwaysShareConfiguration = false)
            => new ContentType(appId, staticName, staticName, 0, "System", description, null, 0, 0,
                alwaysShareConfiguration, null)
            {
                Attributes = attributes
            };

        public const int DynTypeId = 1;
        public const string DynTypeDefScope = Constants.ScopeSystem;
        public const string DynTypeDefDescription = "Dynamic content type";

        public static ContentType Fake(string typeName)
            => DynamicContentType(Constants.TransientAppId, typeName, typeName);

        public static ContentType DynamicContentType(int appId, string typeName, string typeIdentifier, string scope = DynTypeDefScope)
            => new ContentType(appId, typeName, typeIdentifier, DynTypeId, scope, DynTypeDefDescription, null, 0, 0,
                false, null)
            {
                Attributes = new List<IContentTypeAttribute>(),
                IsDynamic = true
            };

        public static void SetSourceAndParent(this ContentType type, RepositoryTypes source, int parentId, string address)
        {
            type.RepositoryType = source;
            type.ParentId = parentId;
            type.RepositoryAddress = address;
        }
    }
}
