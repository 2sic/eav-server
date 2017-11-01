using System;
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
    public class OfMetadataOfItem<T> : IMetadataOfItem
    {
        /// <summary>
        /// initialize using a prepared metadata provider
        /// </summary>
        public OfMetadataOfItem(int itemType, T key, IDeferredEntitiesList metaProvider) : this(itemType, key)
        {
            _appMetadataProvider = metaProvider;
        }

        /// <summary>
        /// initialize using keys to the metadata-environment, for lazy retrieval
        /// </summary>
        public OfMetadataOfItem(int itemType, T key, int remoteZoneId, int remoteAppId): this(itemType, key)
        {
            _remoteZoneId = remoteZoneId;
            _remoteAppId = remoteAppId;
        }

        private OfMetadataOfItem(int itemType, T key)
        {
            _itemType = itemType;
            _key = key;
        }


        private int AppId = 0; // todo: may be a problem, must check!
        private readonly int _remoteAppId;

        private readonly int _remoteZoneId;

        private readonly IDeferredEntitiesList _appMetadataProvider;
        private readonly int _itemType;
        private readonly T _key;
        private List<IEntity> _entities;

        private void LoadFromProvider()
        {
            var metadataProvider = _remoteAppId != 0
                ? Factory.Resolve<IRemoteMetadataProvider>()?.OfZoneAndApp(_remoteZoneId, _remoteAppId)
                : _appMetadataProvider?.Metadata;

            _entities = metadataProvider?.GetMetadata(_itemType, _key).ToList()
                        ?? new List<IEntity>();
        }


        public void Add(string type, Dictionary<string, object> values)
            => (_entities ?? (_entities = new List<IEntity>())).Add(new Entity(AppId, Guid.Empty, type, values));

        public void Use(List<IEntity> items) => _entities = items;

        #region enumerator

        public IEnumerator<IEntity> GetEnumerator()
        {
            // If necessary, initialize first. Note that it will only add Ids which really exist in the source (the source should be the cache)
            if (_entities == null)
                LoadFromProvider();
            return new EntityEnumerator(_entities);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
