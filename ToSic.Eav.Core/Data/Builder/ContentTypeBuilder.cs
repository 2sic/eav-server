using System.Collections.Generic;
using ToSic.Eav.Data.Shared;
using ToSic.Eav.Repositories;

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

        public static void SetSource(this ContentType type, RepositoryTypes repoType)
        {
            type.RepositoryType = repoType;
        }

        public static void SetSourceParentAndIdForPresetTypes(this ContentType type, RepositoryTypes repoType, int parentId, string address, int id = -1)
        {
            if (id != -1) type.ContentTypeId = id;
            type.RepositoryType = repoType;
            type.RepositoryAddress = address;
            var ancestorDecorator = type.GetDecorator<IAncestor>();
            if (ancestorDecorator != null) ancestorDecorator.Id = parentId;
            else type.Decorators.Add(new Ancestor<IContentType>(Constants.PresetIdentity, parentId));
        }
    }
}
