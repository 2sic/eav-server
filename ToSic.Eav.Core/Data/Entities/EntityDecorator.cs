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
    [PrivateApi]
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
            _EntityForEqualityCheck = (Entity as IEntityWrapper)?._EntityForEqualityCheck ?? Entity;
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
        public object GetBestValue(string attributeName, bool resolveHyperlinks = false)
            => Entity.GetBestValue(attributeName, resolveHyperlinks);


        /// <inheritdoc />
        public object GetBestValue(string attributeName, string[] languages, bool resolveHyperlinks = false)
            => Entity.GetBestValue(attributeName, languages, resolveHyperlinks);

        /// <inheritdoc />
        public T GetBestValue<T>(string attributeName, string[] languages, bool resolveHyperlinks = false) 
            => Entity.GetBestValue<T>(attributeName, languages, resolveHyperlinks);

        [PrivateApi]
        public object PrimaryValue(string attributeName, bool resolveHyperlinks = false) 
            => Entity.PrimaryValue(attributeName, resolveHyperlinks);

        [PrivateApi]
        public T PrimaryValue<T>(string attributeName, bool resolveHyperlinks = false) 
            => Entity.PrimaryValue<T>(attributeName, resolveHyperlinks);

        /// <inheritdoc />
        public TVal GetBestValue<TVal>(string name, bool resolveHyperlinks = false)
            => Entity.GetBestValue<TVal>(name, resolveHyperlinks);

        /// <inheritdoc />
        public string GetBestTitle() => Entity.GetBestTitle();

        /// <inheritdoc />
        public string GetBestTitle(string[] dimensions)
            => Entity.GetBestTitle(dimensions);

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


        /// <inheritdoc />
        public List<IEntity> Children(string field = null, string type = null) => Entity.Children(field, type);

        /// <inheritdoc />
        public List<IEntity> Parents(string type = null, string field = null) => Entity.Parents(type, field);


        #region support for LINQ enhancements

        [PrivateApi]
        public object Value(string field, bool resolve = true) => Entity.Value(field, resolve);

        [PrivateApi]
        public T Value<T>(string field, bool resolve = true) => Entity.Value<T>(field, resolve);

        #endregion

    }
}