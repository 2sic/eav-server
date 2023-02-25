using System;

namespace ToSic.Eav.Data.Build
{
    public class EntityPartsBuilder
    {
        private readonly Func<IEntityLight, IRelationshipManager> _getRm;
        public EntityPartsBuilder(Func<IEntityLight, IRelationshipManager> getRelationshipManager)
        {
            _getRm = getRelationshipManager ?? throw new ArgumentNullException(nameof(getRelationshipManager));
        }

        public IRelationshipManager RelationshipManager(IEntityLight entity) => _getRm(entity);
    }
}
