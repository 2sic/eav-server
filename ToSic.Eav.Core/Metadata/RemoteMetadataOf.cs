using System;
using ToSic.Eav.Apps;
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
        internal RemoteMetadataOf(int targetType, T key, int remoteZoneId, int remoteAppId) : base(targetType, key)
        {
            _remoteZoneId = remoteZoneId;
            _remoteAppId = remoteAppId;
        }

        ///// <summary>
        ///// Optional overload in cases where it's not remote after all
        ///// Needed for ContentTypeMetadata, which may be remote or may not be remote
        ///// </summary>
        //internal RemoteMetadataOf(int targetType, T key, IHasMetadataSource metaProvider) : base(targetType, key, metaProvider) { }

        ///// <summary>
        ///// Optional overload in cases where it's not remote after all
        ///// Needed for ContentTypeMetadata, which may be remote or may not be remote
        ///// </summary>
        //internal RemoteMetadataOf(int targetType, T key, Func<IHasMetadataSource> metaSourceFinder) : base(targetType, key, metaSourceFinder)
        //{
        //    //_metaSourceFinder = metaSourceFinder;
        //}
        //private readonly Func<IHasMetadataSource> _metaSourceFinder;

        //protected override IHasMetadataSource AppMetadataProvider => _metaSourceFinder?.Invoke();

        private readonly int _remoteAppId;
        private readonly int _remoteZoneId;

        [PrivateApi]
        protected override IMetadataSource GetMetadataSource()
        {
            // check if already retrieved
            if (_alreadyTriedToGetProvider) return _metadataSource;
            _alreadyTriedToGetProvider = true;

            //if (_metaSourceFinder != null)
            //{
            //    _metadataSource = _metaSourceFinder.Invoke().MetadataSource;
            //    return _metadataSource;
            //}

            _metadataSource = _remoteAppId != 0
                ? (_remoteZoneId != 0
                    ? State.Get(new AppIdentity(_remoteZoneId, _remoteAppId))
                    : State.Get(_remoteAppId))
                : base.GetMetadataSource();
            return _metadataSource;
        }
        private bool _alreadyTriedToGetProvider;
        private IMetadataSource _metadataSource;
        
    }
}
