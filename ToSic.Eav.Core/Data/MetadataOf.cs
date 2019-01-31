using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    /// <inheritdoc/>
    /// <summary>
    /// Metadata entities of an item (a content-type or another entity)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MetadataOf<T> : IMetadataOfItem
    {
        /// <summary>
        /// initialize using a prepared metadata provider
        /// </summary>
        public MetadataOf(int itemType, T key, IDeferredEntitiesList metaProvider) : this(itemType, key)
        {
            _appMetadataProvider = metaProvider;
        }

        /// <summary>
        /// initialize using keys to the metadata-environment, for lazy retrieval
        /// </summary>
        public MetadataOf(int itemType, T key, int remoteZoneId, int remoteAppId): this(itemType, key)
        {
            _remoteZoneId = remoteZoneId;
            _remoteAppId = remoteAppId;
        }

        private MetadataOf(int itemType, T key)
        {
            _itemType = itemType;
            Key = key;
        }


        //private int AppId = 0; 
        private readonly int _remoteAppId;
        private readonly int _remoteZoneId;

        private readonly IDeferredEntitiesList _appMetadataProvider;
        private readonly int _itemType;
        protected readonly T Key;

        /// <summary>
        /// All entities is internal - because it contains everything
        /// including permissions-metadata
        /// </summary>
        protected List<IEntity> AllEntities {
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
                    _filteredEntities = AllEntities
                        .Where(md => new[] { Constants.PermissionTypeName }.Any(e => e != md.Type.Name && e != md.Type.StaticName))
                        .ToList();
                return _filteredEntities;
            }
        }
        private List<IEntity> _filteredEntities;

        public IEnumerable<IEntity> Permissions =>
            AllEntities.Where(md => md.Type.StaticName == Constants.PermissionTypeName);

        private long _cacheTimestamp;

        protected bool RequiresReload()
            => _metadataProvider != null && _metadataProvider.CacheChanged(_cacheTimestamp);
        

        protected virtual void LoadFromProvider()
        {
            var mdProvider = GetMetadataProvider();
            Use(mdProvider?.GetMetadata(_itemType, Key).ToList()
                       ?? new List<IEntity>());
            if (mdProvider != null)
                _cacheTimestamp = mdProvider.CacheTimestamp;
        }

        protected IMetadataProvider GetMetadataProvider()
        {
            // check if already retrieved
            if (_alreadyTriedToGetProvider) return _metadataProvider;

            _metadataProvider = _remoteAppId != 0
                ? (_remoteZoneId != 0
                    ? Factory.Resolve<IRemoteMetadataProvider>()?.OfZoneAndApp(_remoteZoneId, _remoteAppId)
                    : Factory.Resolve<IRemoteMetadataProvider>()?.OfApp(_remoteAppId))
                : _appMetadataProvider?.Metadata;
            _alreadyTriedToGetProvider = true;
            return _metadataProvider;
        }
        private bool _alreadyTriedToGetProvider;
        private IMetadataProvider _metadataProvider;

        // 2018-03-09 2dm - this was used when we tried creating code-based content-types, but I believe it's dead code now
        //public void Add(string type, Dictionary<string, object> values) => Add(new Entity(AppId, 0, Guid.Empty, type, values));

        public void Add(IEntity additionalItem) => AllEntities.Add(additionalItem);

        public void Use(List<IEntity> items)
        {
            _allEntities = items;
            _filteredEntities = null; // ensure this will be re-built when accessed
        }

        public TVal GetBestValue<TVal>(string name, string type = null)
        {
            var list = type == null ? this : this.Where(md => md.Type.StaticName == type || md.Type.Name == type);
            var found = list.FirstOrDefault(md => md.Attributes.ContainsKey(name));
            return found == null ? default(TVal) : found.GetBestValue<TVal>(name);
        }

        public TVal GetBestValue<TVal>(string name, string[] types)
        {
            foreach (var type in types)
            {
                var result = GetBestValue<TVal>(name, type);
                if (!EqualityComparer<TVal>.Default.Equals(result, default(TVal)))
                    return result;
            }
            return default(TVal);
        }

        #region enumerators
        public IEnumerator<IEntity> GetEnumerator() => new EntityEnumerator(FilteredEntities);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
