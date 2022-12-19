using System;
using System.Collections.Generic;
using ToSic.Eav.Metadata;
using ToSic.Lib.Documentation;

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

        [PrivateApi] public IEntity RootContentsForEqualityCheck => (Entity as IEntityWrapper)?.RootContentsForEqualityCheck ?? Entity;
        public List<IDecorator<IEntity>> Decorators => _decorators ?? (_decorators = (Entity as IEntityWrapper)?.Decorators ?? new List<IDecorator<IEntity>>());
        private List<IDecorator<IEntity>> _decorators;

        /// <summary>
        /// Create a EntityBasedType and wrap the entity provided
        /// </summary>
        /// <param name="entity"></param>
        protected EntityBasedType(IEntity entity) => Entity = entity;

        protected EntityBasedType(IEntity entity, string[] languageCodes) : this(entity)
            => LookupLanguages = languageCodes ?? Array.Empty<string>();

        protected EntityBasedType(IEntity entity, string languageCode) : this(entity) 
            => LookupLanguages = languageCode != null ? new[] {languageCode} : Array.Empty<string>();

        /// <inheritdoc />
        public virtual string Title => _title ?? (_title = Entity?.GetBestTitle() ?? "");
        private string _title;

        /// <inheritdoc />
        public int Id => Entity?.EntityId ?? 0;

        /// <inheritdoc />
        public Guid Guid => Entity?.EntityGuid ?? Guid.Empty;

        /// <inheritdoc />
        public IMetadataOf Metadata => Entity?.Metadata;

        [PrivateApi]
        protected string[] LookupLanguages { get; } = Array.Empty<string>();


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
