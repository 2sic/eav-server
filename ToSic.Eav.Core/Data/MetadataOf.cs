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


        private int AppId = 0; // todo: may be a problem, must check!
        private readonly int _remoteAppId;

        private readonly int _remoteZoneId;

        private readonly IDeferredEntitiesList _appMetadataProvider;
        private readonly int _itemType;
        protected readonly T Key;
        protected List<IEntity> Entities;

        protected virtual IMetadataProvider LoadFromProvider()
        {
            var mdProvider = GetMetadataProvider();
            Entities = mdProvider?.GetMetadata(_itemType, Key).ToList()
                        ?? new List<IEntity>();
            return mdProvider;
        }

        private IMetadataProvider GetMetadataProvider()
        {
            var metadataProvider = _remoteAppId != 0
                ? (_remoteZoneId != 0
                    ? GetRemoteMdProvider()?.OfZoneAndApp(_remoteZoneId, _remoteAppId)
                    : GetRemoteMdProvider()?.OfApp(_remoteAppId))
                : _appMetadataProvider?.Metadata;
            return metadataProvider;
        }

        private static IRemoteMetadataProvider GetRemoteMdProvider() 
            => Factory.Resolve<IRemoteMetadataProvider>();


        public void Add(string type, Dictionary<string, object> values)
            => Add(new Entity(AppId, Guid.Empty, type, values));

        public void Add(IEntity additionalItem)
            => (Entities ?? (Entities = new List<IEntity>())).Add(additionalItem);


        public void Use(List<IEntity> items) => Entities = items;

        #region enumerator

        public IEnumerator<IEntity> GetEnumerator()
        {
            // If necessary, initialize first. Note that it will only add Ids which really exist in the source (the source should be the cache)
            if (Entities == null)
                LoadFromProvider();
            return new EntityEnumerator(Entities);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
