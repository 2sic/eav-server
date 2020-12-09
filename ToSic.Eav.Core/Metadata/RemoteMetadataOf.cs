using ToSic.Eav.Documentation;

namespace ToSic.Eav.Metadata
{
    [PrivateApi("the remote-concept is only used for ghost types, and that's fairly internal")]
    public class RemoteMetadataOf<T>: MetadataOf<T>
    {
        /// <summary>
        /// initialize using keys to the metadata-environment, for lazy retrieval
        /// The remote mode is for internal use only, as it's a left-over of ghost content-types, which we don't want to
        /// promote any more like this.
        /// </summary>
        internal RemoteMetadataOf(int itemType, T key, int remoteZoneId, int remoteAppId) : base(itemType, key)
        {
            _remoteZoneId = remoteZoneId;
            _remoteAppId = remoteAppId;
        }

        /// <summary>
        /// Optional overload in cases where it's not remote after all
        /// Needed for ContentTypeMetadata, which may be remote or may not be remote
        /// </summary>
        internal RemoteMetadataOf(int itemType, T key, IHasMetadataSource metaProvider) : base(itemType, key, metaProvider) { }

        private readonly int _remoteAppId;
        private readonly int _remoteZoneId;

        [PrivateApi]
        protected override IMetadataSource GetMetadataSource()
        {
            // check if already retrieved
            if (_alreadyTriedToGetProvider) return _metadataSource;

            _metadataSource = _remoteAppId != 0
                ? (_remoteZoneId != 0
                    ? Factory.Resolve<IRemoteMetadata>()?.OfZoneAndApp(_remoteZoneId, _remoteAppId)
                    : Factory.Resolve<IRemoteMetadata>()?.OfApp(_remoteAppId))
                : base.GetMetadataSource();
            _alreadyTriedToGetProvider = true;
            return _metadataSource;
        }
        private bool _alreadyTriedToGetProvider;
        private IMetadataSource _metadataSource;

    }
}
