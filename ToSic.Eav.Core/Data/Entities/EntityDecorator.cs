using System;
using System.Collections.Generic;
using ToSic.Eav.Documentation;
using ToSic.Eav.Metadata;
using ToSic.Eav.Security;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// This extends an existing <see cref="IEntity"/> with more properties and information. 
    /// Everything in the original is passed through invisibly. <br/>
    /// </summary>
    [PrivateApi("this decorator object is for internal use only, no value in publishing it")]
    public abstract partial class EntityDecorator : IEntity, IEntityWrapper
    {
        public IEntity Entity { get; }

        /// <summary>
        /// Initialize the object and store the underlying IEntity.
        /// </summary>
        /// <param name="baseEntity"></param>
        protected EntityDecorator(IEntity baseEntity)
        {
            Entity = baseEntity;
            EntityForEqualityCheck = (Entity as IEntityWrapper)?.EntityForEqualityCheck ?? Entity;
        }

        #region IEntity Implementation

        /// <inheritdoc />
        public IEntity GetDraft() => Entity.GetDraft();

        /// <inheritdoc />
        public IEntity GetPublished() => Entity.GetPublished();

        /// <inheritdoc />
        public int AppId => Entity.AppId;

        /// <inheritdoc />
        public int EntityId => Entity.EntityId;

        /// <inheritdoc />
        public int RepositoryId => Entity.RepositoryId;

        /// <inheritdoc />
        public Guid EntityGuid => Entity.EntityGuid;

        /// <inheritdoc />
        public ITarget MetadataFor => Entity.MetadataFor;

        /// <inheritdoc />
        public Dictionary<string, IAttribute> Attributes => Entity.Attributes;

        /// <inheritdoc />
        public IContentType Type => Entity.Type;

        /// <inheritdoc />
        public IAttribute Title => Entity.Title;

        /// <inheritdoc />
        public DateTime Modified => Entity.Modified;

         /// <inheritdoc />
       public IAttribute this[string attributeName] => Entity[attributeName];

        /// <inheritdoc />
        public IRelationshipManager Relationships => Entity.Relationships;

        /// <inheritdoc />
        public bool IsPublished => Entity.IsPublished;

        /// <inheritdoc />
        public string Owner => Entity.Owner;

        /// <inheritdoc />
        public object GetBestValue(string attributeName, string[] languages) => Entity.GetBestValue(attributeName, languages);

        /// <inheritdoc />
        public T GetBestValue<T>(string attributeName, string[] languages) => Entity.GetBestValue<T>(attributeName, languages);

        /// <inheritdoc />
        public string GetBestTitle() => Entity.GetBestTitle();

        /// <inheritdoc />
        public string GetBestTitle(string[] dimensions) => Entity.GetBestTitle(dimensions);

        /// <inheritdoc />
        object IEntityLight.Title => ((IEntityLight) Entity).Title;

        /// <inheritdoc />
        object IEntityLight.this[string attributeName] => ((IEntityLight) Entity)[attributeName];

        /// <inheritdoc />
        public int Version => Entity.Version;

        /// <inheritdoc />
        public IMetadataOf Metadata => Entity.Metadata;

        /// <inheritdoc />
        public IEnumerable<Permission> Permissions => Entity.Permissions;

        #endregion

        #region support for LINQ enhancements

        /// <inheritdoc />
        public List<IEntity> Children(string field = null, string type = null) => Entity.Children(field, type);

        /// <inheritdoc />
        public List<IEntity> Parents(string type = null, string field = null) => Entity.Parents(type, field);

        #endregion

        /// <inheritdoc />
        public object Value(string field) => Entity.Value(field);

        /// <inheritdoc />
        public T Value<T>(string field) => Entity.Value<T>(field);
    }
}