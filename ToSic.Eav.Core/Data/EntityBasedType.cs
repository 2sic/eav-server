using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Foundation for a class which gets its data from an Entity. <br/>
    /// This is used for more type safety - because some internal objects need entities for data-storage,
    /// but when programming they should use typed objects to not accidentally access invalid properties. 
    /// </summary>
    [PublicApi]
    public abstract class EntityBasedType : IEntityBasedType
    {
        /// <inheritdoc />
        public IEntity Entity { get; }

         /// <inheritdoc />
        protected EntityBasedType(IEntity entity) => Entity = entity;

         /// <inheritdoc />
        public virtual string Title => _title ?? (_title = Entity?.GetBestTitle());
        [PrivateApi]
        protected string _title;

         /// <inheritdoc />
        public int Id => Entity?.EntityId ?? 0;

         /// <inheritdoc />
        public Guid Guid => Entity?.EntityGuid ?? Guid.Empty;
    }
}
