using System;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Temporary, name / etc. still WIP!
    /// Maybe better
    /// - EntityTyped
    /// - TypedEntity
    /// - SafeEntity
    /// - SafeEntityBase
    /// - ???
    /// </summary>
    public abstract class EntityBasedType
    {
        public readonly IEntity Entity;

        protected EntityBasedType(IEntity entity)
        {
            Entity = entity;
        }

        public virtual string Title
        {
            get => _name ?? (_name = Entity?.GetBestTitle());
            set => _name = value;
        }
        private string _name;

        public int Id => Entity?.EntityId ?? 0;
        public Guid Guid => Entity?.EntityGuid ?? Guid.Empty;
    }
}
