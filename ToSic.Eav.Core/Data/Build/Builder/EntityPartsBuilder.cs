using System;
using System.Collections.Generic;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data.Build
{
    public class EntityPartsBuilder
    {
        private readonly Func<IEntityLight, IRelationshipManager> _getRm;
        internal readonly Func<Guid, string, IMetadataOf> GetMetadataOf;

        public EntityPartsBuilder(
            Func<IEntityLight, IRelationshipManager> getRelationshipManager,
            Func<Guid, string, IMetadataOf> getMetadataOf = null)
        {
            _getRm = getRelationshipManager ?? throw new ArgumentNullException(nameof(getRelationshipManager));
            GetMetadataOf = getMetadataOf ?? EmptyGetMetadataOf;
        }

        private IMetadataOf EmptyGetMetadataOf(Guid guid, string title) => new MetadataOf<Guid>(targetType: (int)TargetTypes.Entity, key: guid, title: title);

        public static Func<Guid, string, IMetadataOf> CreateMetadataOfAppSources(IHasMetadataSource appSource)
            => (guid, title) => new MetadataOf<Guid>(targetType: (int)TargetTypes.Entity, key: guid, title: title,
                appSource: appSource);

        public static Func<Guid, string, IMetadataOf> CreateMetadataOfItems(List<IEntity> items)
            => (guid, title) => new MetadataOf<Guid>(targetType: (int)TargetTypes.Entity, key: guid, title: title, items: items);

        public static Func<TKey, string, IMetadataOf> ReUseMetadataFunc<TKey>(IMetadataOf original) 
            => (key, title) => original;

        public static Func<TKey, string, IMetadataOf> CloneMetadataFunc<TKey>(IMetadataOf original,
            List<IEntity> items = default,
            IHasMetadataSource appSource = default,
            Func<IHasMetadataSource> deferredSource = default
            )
        {
            var specs = ((IMetadataInternals)original).GetCloneSpecs();
            return (key, title) => new MetadataOf<TKey>(targetType: specs.TargetType, key: key, title: title,
                items: items ?? specs.list,
                appSource: appSource ?? specs.appSource,
                deferredSource: deferredSource ?? specs.deferredSource);
        }

        public IRelationshipManager RelationshipManager(IEntityLight entity) => _getRm(entity);
    }
}
