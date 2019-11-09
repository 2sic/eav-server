using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Foundation for a class which gets its data from an Entity. <br/>
    /// This is used for more type safety - because some internal objects need entities for data-storage,
    /// but when programming they should use typed objects to not accidentally access invalid properties. 
    /// </summary>
    // todo: last chance for a final name
    [PublicApi]
    public abstract class EntityBasedType
    {
        /// <summary>
        /// The underlying entity. 
        /// </summary>
        public readonly IEntity Entity;

        /// <summary>
        /// Constructor, needs the underlying entity from which it will usually read. 
        /// </summary>
        /// <param name="entity">The underlying entity to use</param>
        protected EntityBasedType(IEntity entity) => Entity = entity;

        /// <summary>
        /// The title as string.
        /// </summary>
        /// <remarks>Can be overriden by other parts, if necessary.</remarks>
        public virtual string Title => _title ?? (_title = Entity?.GetBestTitle());
        [PrivateApi]
        protected string _title;

        /// <summary>
        /// The entity id, as quick, nice accessor.
        /// </summary>
        public int Id => Entity?.EntityId ?? 0;

        /// <summary>
        /// The entity guid, as quick, nice accessor. 
        /// </summary>
        public Guid Guid => Entity?.EntityGuid ?? Guid.Empty;
    }
}
