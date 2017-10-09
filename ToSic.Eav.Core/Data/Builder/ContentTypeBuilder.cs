using System.Collections.Generic;
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
                alwaysShareConfiguration)
            {
                Attributes = attributes
            };

        public const int DynTypeId = 1;
        public const string DynTypeDefScope = Constants.ScopeSystem;
        public const string DynTypeDefDescription = "Dynamic content type";

        public static ContentType DynamicContentType(int appId, string scope = DynTypeDefScope)
            => new ContentType(appId, Constants.DynamicType, Constants.DynamicType, DynTypeId, scope, DynTypeDefDescription, null, 0, 0,
                false)
            {
                Attributes = new List<IAttributeDefinition>()
            };

    }
}
