using System.Collections.Generic;
using ToSic.Eav.Data.Shared;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Data.Builder
{
    public static class ContentTypeBuilder
    {
        // 2021-11-22 2dm removed, seems unused - #cleanup EOY 2021
        ///// <summary>
        ///// Shortcut go get a new AttributeSet with Scope=System and Name=StaticName
        ///// </summary>
        //public static ContentType SystemAttributeSet(int appId, string staticName, string description,
        //    List<IContentTypeAttribute> attributes, bool alwaysShareConfiguration = false)
        //    => new ContentType(appId, staticName, staticName, 0, Constants.ScopeSystem, description, null, 0, 0,
        //        alwaysShareConfiguration)
        //    {
        //        Attributes = attributes
        //    };

        public const int DynTypeId = 1;
        public const string DynTypeDefScope = Constants.ScopeSystem;
        public const string DynTypeDefDescription = "Dynamic content type";

        public static ContentType Fake(string typeName)
            => DynamicContentType(Constants.TransientAppId, typeName, typeName);

        public static ContentType DynamicContentType(int appId, string typeName, string typeIdentifier, string scope = DynTypeDefScope)
            => new ContentType(appId, typeName, typeIdentifier, DynTypeId, scope, DynTypeDefDescription)
            {
                Attributes = new List<IContentTypeAttribute>(),
                IsDynamic = true
            };

        public static void SetSource(this ContentType type, RepositoryTypes repoType)
        {
            type.RepositoryType = repoType;
        }

        public static void SetSourceAndParent(this ContentType type, RepositoryTypes repoType, int parentId, string address)
        {
            type.RepositoryType = repoType;
            //type.ParentId = parentId;
            type.RepositoryAddress = address;
            var ancestorDecorator = type.GetDecorator<IAncestor>();
            if (ancestorDecorator != null) ancestorDecorator.Id = parentId;
            else type.Decorators.Add(new Ancestor<IContentType>(Constants.PresetIdentity, parentId));
        }
    }
}
