using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Security;

namespace ToSic.Eav.Metadata
{
    /// <summary>
    /// Metadata of an item (a content-type or another entity). <br/>
    /// It's usually on a <strong>Metadata</strong> property of things that can have metadata.
    /// </summary>
    /// <typeparam name="T">The type this metadata uses as a key - int, string, guid</typeparam>
    [PrivateApi] // changed 2020-12-09 v11.11 from [PublicApi_Stable_ForUseInYourCode] - as this is a kind of lazy-metadata, we should change it to that
    public class MetadataOf<T> : IMetadataOf, IMetadataInternals, ITimestamped
    {

        #region Constructors

        /// <summary>
        /// initialize using an already prepared metadata source
        /// </summary>
        public MetadataOf(int targetType, T key, IHasMetadataSource metaProvider, string targetIdentifier) : this(targetType, key, targetIdentifier)
        {
            _appMetadataProvider = metaProvider;
        }

        /// <summary>
        /// initialize using an already prepared metadata source
        /// </summary>
        public MetadataOf(int targetType, T key, Func<IHasMetadataSource> metaSourceRemote, string targetIdentifier) : this(targetType, key, targetIdentifier)
        {
            _metaSourceRemote = metaSourceRemote;
        }

        /// <summary>
        /// Inner constructor, primarily needed by this and inheriting classes
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="key"></param>
        private MetadataOf(int targetType, T key, string targetIdentifier)
        {
            _targetType = targetType;
            Key = key;
            _metadataIdentifier = targetIdentifier;
        }

        #endregion

        /// <summary>
        /// The source (usually an app) which can provide all the metadata once needed
        /// </summary>
        protected virtual IHasMetadataSource AppMetadataProvider => _appMetadataProvider ?? _metaSourceRemote?.Invoke();

        private readonly IHasMetadataSource _appMetadataProvider;
        private readonly Func<IHasMetadataSource> _metaSourceRemote;


        /// <summary>
        /// Type-information of the thing we're describing. This is used to retrieve metadata from the correct sub-list of pre-indexed metadata
        /// </summary>
        private readonly int _targetType;

        /// <summary>
        /// The key which identifies the item we're enriching with metadata
        /// </summary>
        public T Key { get; }

        /// <summary>
        /// All entities is internal - because it contains everything
        /// including permissions-metadata
        /// </summary>
        [PrivateApi]
        public List<IEntity> AllWithHidden {
            get
            {             
                // If necessary, initialize first. Note that it will only add Ids which really exist in the source (the source should be the cache)
                if (_allEntities == null || RequiresReload())
                    LoadFromProvider();
                return _allEntities;
            }
        }
        private List<IEntity> _allEntities;

        /// <summary>
        /// All "normal" metadata entities - so it hides the system-entities
        /// like permissions. This is the default view of metadata given by an item
        /// </summary>
        private List<IEntity> MetadataWithoutPermissions
        {
            get
            {
                // If necessary, initialize first. Note that it will only add Ids which really exist in the source (the source should be the cache)
                if (_metadataWithoutPermissions == null || RequiresReload())
                    _metadataWithoutPermissions = AllWithHidden
                        // 2021-11-19 changed to use Permission.IsPermission - if stable #cleanup EOY 2021
                        .Where(md => !Permission.IsPermission(md))
                        //.Where(md => new[] {Permission.TypeName  }.Any(e => e != md.Type.Name && e != md.Type.StaticName))
                        .ToList();
                return _metadataWithoutPermissions;
            }
        }
        private List<IEntity> _metadataWithoutPermissions;

        /// <inheritdoc />
        public IEnumerable<Permission> Permissions
        {
            get
            {
                if (_permissions == null || RequiresReload())
                    _permissions = AllWithHidden
                        // 2021-11-19 changed to use Permission.IsPermission - if stable #cleanup EOY 2021
                        .Where(Permission.IsPermission)
                        //.Where(md => md.Type.StaticName == Permission.TypeName)
                        .Select(e => new Permission(e));
                return _permissions;
            }
        }
        private IEnumerable<Permission> _permissions;

        public long CacheTimestamp { get; private set; }

        [PrivateApi]
        protected bool RequiresReload() => GetMetadataSource()?.CacheChanged(CacheTimestamp) == true;
        
        /// <summary>
        /// Load the metadata from the provider
        /// Must be virtual, because the inheriting <see cref="ContentTypeMetadata"/> needs to overwrite this. 
        /// </summary>
        [PrivateApi]
        protected virtual void LoadFromProvider()
        {
            var mdProvider = GetMetadataSource();
            Use(mdProvider?.GetMetadata(_targetType, Key).ToList() ?? new List<IEntity>());
            if (mdProvider != null)
                CacheTimestamp = mdProvider.CacheTimestamp;
        }

        /// <summary>
        /// Find the source of metadata. It may already be set, or it may not be available at all.
        /// </summary>
        /// <returns></returns>
        [PrivateApi]
        protected IMetadataSource GetMetadataSource()
        {
            // check if already retrieved
            if (_alreadyTriedToGetSource) return _metadataSource;
            _alreadyTriedToGetSource = true;

            return _metadataSource = AppMetadataProvider?.MetadataSource;
        }
        private bool _alreadyTriedToGetSource;
        private IMetadataSource _metadataSource;

        /// <summary>
        /// Set the local cache to a list of items to use as Metadata.
        /// </summary>
        /// <param name="items"></param>
        [PrivateApi]
        public void Use(List<IEntity> items)
        {
            // Set the local cache to a list of items, and reset the dependent objects so they will be rebuilt if accessed.
            _allEntities = items;
            _metadataWithoutPermissions = null;
            _permissions = null;
        }

        #region GetBestValue

        /// <inheritdoc />
        public TVal GetBestValue<TVal>(string name, string typeName = null)
        {
            var list = typeName == null ? this : this.Where(md => md.Type.Is(typeName));
            var found = list.FirstOrDefault(md => md.Attributes.ContainsKey(name));
            return found == null ? default : found.GetBestValue<TVal>(name, null);
        }

        /// <inheritdoc />
        public TVal GetBestValue<TVal>(string name, string[] typeNames)
        {
            foreach (var type in typeNames)
            {
                var result = GetBestValue<TVal>(name, type);
                if (!EqualityComparer<TVal>.Default.Equals(result, default))
                    return result;
            }
            return default;
        }
        #endregion

        #region Type Specific Data

        [PrivateApi("WIP 12.10")]
        public bool HasType(string name) => this.Any(e => e.Type.Is(name));

        [PrivateApi("WIP 12.10")]
        public IEnumerable<IEntity> OfType(string name) => this.Where(e => e.Type.Is(name));

        #endregion

        #region Target

        public ITarget MetadataId
        {
            get
            {
                if (_target != null) return _target;
                var target = new Target(_targetType, _metadataIdentifier);
                if (Key is string stringKey) target.KeyString = stringKey;
                if (Key is int intKey) target.KeyNumber = intKey;
                if (Key is Guid guidKey) target.KeyGuid = guidKey;
                return _target = target;
            }
        }

        private ITarget _target;
        private readonly string _metadataIdentifier;

        #endregion


        #region enumerators
        [PrivateApi]
        public IEnumerator<IEntity> GetEnumerator() => new EntityEnumerator(MetadataWithoutPermissions);

        [PrivateApi]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
