//using ToSic.Eav.Data.Shared;
//using ToSic.Eav.Repositories;

//namespace ToSic.Eav.Data.Builder
//{
//    public static class ContentTypeExtensions
//    {

//        public static void SetSourceParentAndIdForPresetTypes(this ContentType type, RepositoryTypes repoType, int parentId, string address, int id)
//        {
//            //if (id != -1) type.Id = id;
//            type.Id = id;
//            type.RepositoryType = repoType;
//            type.RepositoryAddress = address;
//            var ancestorDecorator = type.GetDecorator<IAncestor>();
//            if (ancestorDecorator != null) ancestorDecorator.Id = parentId;
//            else type.Decorators.Add(new Ancestor<IContentType>(Constants.PresetIdentity, parentId));
//        }
//    }
//}
