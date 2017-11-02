using System.Collections.Generic;
using ToSic.Eav.Enums;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data.Builder
{
    public static class ContentTypeBuilder
    {
        /// <summary>
        /// Shortcut go get a new AttributeSet with Scope=System and Name=StaticName
        /// </summary>
        public static ContentType SystemAttributeSet(int appId, string staticName, string description,
            List<IAttributeDefinition> attributes, bool alwaysShareConfiguration = false)
            => new ContentType(appId, staticName, staticName, 0, "System", description, null, 0, 0,
                alwaysShareConfiguration, null)
            {
                Attributes = attributes
            };

        public const int DynTypeId = 1;
        public const string DynTypeDefScope = Constants.ScopeSystem;
        public const string DynTypeDefDescription = "Dynamic content type";

        public static ContentType DynamicContentType(int appId, string typeName, string scope = DynTypeDefScope)
            => new ContentType(appId, typeName, typeName, DynTypeId, scope, DynTypeDefDescription, null, 0, 0,
                false, null)
            {
                Attributes = new List<IAttributeDefinition>(),
                IsDynamic = true
            };

        public static void SetSourceAndParent(this ContentType type, Repositories source, int parentId)
        {
            type.Source = source;
            type.ParentId = parentId;
        }
    }
}
