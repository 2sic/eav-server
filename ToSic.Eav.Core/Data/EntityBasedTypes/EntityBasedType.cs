using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Foundation for a class which gets its data from an Entity. <br/>
    /// This is used for more type safety - because some internal objects need entities for data-storage,
    /// but when programming they should use typed objects to not accidentally access invalid properties. 
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public abstract class EntityBasedType : IEntityBasedType
    {
        /// <inheritdoc />
        public IEntity Entity { get; }

         /// <inheritdoc />
        protected EntityBasedType(IEntity entity) => Entity = entity;

         /// <inheritdoc />
        public virtual string Title => _title ?? (_title = Entity?.GetBestTitle() ?? "");
        [PrivateApi]
        private string _title;

         /// <inheritdoc />
        public int Id => Entity?.EntityId ?? 0;

         /// <inheritdoc />
        public Guid Guid => Entity?.EntityGuid ?? Guid.Empty;

        /// <summary>
        /// Get a value from the underlying entity. 
        /// </summary>
        /// <typeparam name="T">type, should only be string, decimal, bool</typeparam>
        /// <param name="fieldName">field name</param>
        /// <param name="fallback">fallback value</param>
        /// <returns>The value. If the Entity is missing, will return the fallback result. </returns>
        protected T Get<T>(string fieldName, T fallback)
         {
             if (Entity == null) return fallback;
             var result = Entity.GetBestValue<T>(fieldName);
             if (result == null) return fallback;
             return result;
         }
    }
}
