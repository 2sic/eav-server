using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public class MetadataOf<T> : IMetadataOf, IMetadataInternals
    {
        /// <summary>
        /// initialize using a prepared metadata source
        /// </summary>
        public MetadataOf(int itemType, T key, IHasMetadataSource metaProvider) : this(itemType, key)
        {
            _appMetadataProvider = metaProvider;
        }

        ///// <summary>
        ///// initialize using keys to the metadata-environment, for lazy retrieval
        ///// The remote mode is for internal use only, as it's a left-over of ghost content-types, which we don't want to
        ///// promote any more like this.
        ///// </summary>
        //internal MetadataOf(int itemType, T key, int remoteZoneId, int remoteAppId) : this(itemType, key)
        //{
        //    _remoteZoneId = remoteZoneId;
        //    _remoteAppId = remoteAppId;
        //}

        protected MetadataOf(int itemType, T key)
        {
            _itemType = itemType;
            Key = key;
        }

        //private readonly int _remoteAppId;
        //private readonly int _remoteZoneId;

        private readonly IHasMetadataSource _appMetadataProvider;
        private readonly int _itemType;

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
        private List<IEntity> FilteredEntities
        {
            get
            {
                // If necessary, initialize first. Note that it will only add Ids which really exist in the source (the source should be the cache)
                if (_filteredEntities == null || RequiresReload())
                    _filteredEntities = AllWithHidden
                        .Where(md => new[] {Permission.TypeName  }.Any(e => e != md.Type.Name && e != md.Type.StaticName))
                        .ToList();
                return _filteredEntities;
            }
        }
        private List<IEntity> _filteredEntities;

        /// <inheritdoc />
        public IEnumerable<Permission> Permissions
        {
            get
            {
                if(_permissions == null || RequiresReload())
                    _permissions = AllWithHidden
                               .Where(md => md.Type.StaticName == Permission.TypeName)
                               .Select(e => new Permission(e));
                return _permissions;
            }
        }

        private IEnumerable<Permission> _permissions;

        private long _cacheTimestamp;

        [PrivateApi]
        protected bool RequiresReload()
            => _metadataSource != null && _metadataSource.CacheChanged(_cacheTimestamp);
        
        [PrivateApi]
        protected virtual void LoadFromProvider()
        {
            var mdProvider = GetMetadataSource();
            Use(mdProvider?.GetMetadata(_itemType, Key).ToList()
                       ?? new List<IEntity>());
            if (mdProvider != null)
                _cacheTimestamp = mdProvider.CacheTimestamp;
        }

        [PrivateApi]
        protected virtual IMetadataSource GetMetadataSource()
        {
            // check if already retrieved
            if (_alreadyTriedToGetProvider) return _metadataSource;

            _metadataSource = 
                //_remoteAppId != 0
                //? (_remoteZoneId != 0
                //    ? Factory.Resolve<IRemoteMetadata>()?.OfZoneAndApp(_remoteZoneId, _remoteAppId)
                //    : Factory.Resolve<IRemoteMetadata>()?.OfApp(_remoteAppId))
                //: 
                _appMetadataProvider?.MetadataSource;
            _alreadyTriedToGetProvider = true;
            return _metadataSource;
        }
        private bool _alreadyTriedToGetProvider;
        private IMetadataSource _metadataSource;

        [PrivateApi]
        public void Use(List<IEntity> items)
        {
            _allEntities = items;
            _filteredEntities = null; // ensure this will be re-built when accessed
        }

        #region GetBestValue

        /// <inheritdoc />
        public TVal GetBestValue<TVal>(string name, string typeName = null)
        {
            var list = typeName == null ? this : this.Where(md => md.Type.Is(typeName)); //.StaticName == typeName || md.Type.Name == typeName);
            var found = list.FirstOrDefault(md => md.Attributes.ContainsKey(name));
            return found == null ? default : found.GetBestValue<TVal>(name);
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

        #region enumerators
        [PrivateApi]
        public IEnumerator<IEntity> GetEnumerator() => new EntityEnumerator(FilteredEntities);

        [PrivateApi]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
