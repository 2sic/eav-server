using System;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Temporary, name / etc. still WIP!
    /// </summary>
    public abstract class EntityBasedType
    {
        public readonly IEntity Entity;

        protected EntityBasedType(IEntity entity)
        {
            Entity = entity;
        }

        public virtual string EntityTitle
        {
            get => _name ?? (_name = Entity?.GetBestTitle());
            set => _name = value;
        }
        private string _name;

        public int EntityId => Entity?.EntityId ?? 0;
        public Guid EntityGuid => Entity?.EntityGuid ?? Guid.Empty;
    }
}
