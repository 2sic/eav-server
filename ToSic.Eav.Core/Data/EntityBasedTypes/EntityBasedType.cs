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
        public IEntity Entity { get; protected set; }

        [PrivateApi] public IEntity EntityForEqualityCheck => (Entity as IEntityWrapper)?.EntityForEqualityCheck ?? Entity;

        /// <summary>
        /// Create a EntityBasedType and wrap the entity provided
        /// </summary>
        /// <param name="entity"></param>
        protected EntityBasedType(IEntity entity)
        {
            Entity = entity;    
            //EntityForEqualityCheck = (Entity as IEntityWrapper)?.EntityForEqualityCheck ?? Entity;
        }

        protected EntityBasedType(IEntity entity, string[] languageCodes) : this(entity)
            => LookupLanguages = languageCodes ?? new string[0];

        protected EntityBasedType(IEntity entity, string languageCode) : this(entity) 
            => LookupLanguages = languageCode != null ? new[] {languageCode} : new string[0];

        /// <inheritdoc />
        public virtual string Title => _title ?? (_title = Entity?.GetBestTitle() ?? "");
        private string _title;

         /// <inheritdoc />
        public int Id => Entity?.EntityId ?? 0;

         /// <inheritdoc />
        public Guid Guid => Entity?.EntityGuid ?? Guid.Empty;

        [PrivateApi]
        protected string[] LookupLanguages { get; } = new string[0];


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
            var result = Entity.GetBestValue<T>(fieldName, LookupLanguages);
            if (result == null) return fallback;
            return result;
        }
    }
}
