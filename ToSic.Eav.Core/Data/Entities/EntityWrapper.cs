using System;
using System.Collections.Generic;
using ToSic.Eav.Data.Debug;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Security;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// This extends an existing <see cref="IEntity"/> with more properties and information. 
    /// Everything in the original is passed through invisibly. <br/>
    /// </summary>
    [PrivateApi("this decorator object is for internal use only, no value in publishing it")]
    public abstract partial class EntityWrapper : IEntity, IEntityWrapper
    {
        public IEntity Entity { get; }

        /// <summary>
        /// Initialize the object and store the underlying IEntity.
        /// </summary>
        /// <param name="baseEntity"></param>
        protected EntityWrapper(IEntity baseEntity)
        {
            Entity = baseEntity;
            EntityForEqualityCheck = Entity;
            if (Entity is IEntityWrapper wrapper)
            {
                EntityForEqualityCheck = wrapper.EntityForEqualityCheck ?? Entity;
                Decorators.AddRange(wrapper.Decorators);
            }
        }

        /// <summary>
        /// Initialize the object and store the underlying IEntity. - adding an additional decorator
        /// </summary>
        /// <param name="baseEntity"></param>
        /// <param name="decorator">Additional wrapper to add</param>
        protected EntityWrapper(IEntity baseEntity, IDecorator<IEntity> decorator) : this(baseEntity)
        {
            if(decorator != null) Decorators.Add(decorator);
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

        public DateTime Created => Entity.Created;

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

        [PrivateApi("Internal")]
        public virtual PropertyRequest FindPropertyInternal(string field, string[] languages, ILog parentLogOrNull) => Entity.FindPropertyInternal(field, languages, parentLogOrNull);

        [PrivateApi("Internal")]
        public List<PropertyDumpItem> _Dump(string[] languages, string path, ILog parentLogOrNull) => Entity._Dump(languages, path, parentLogOrNull);

        public List<IDecorator<IEntity>> Decorators { get; } = new List<IDecorator<IEntity>>();
    }
}