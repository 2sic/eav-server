using System;
using ToSic.Eav.Caching;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Data.Source
{
    [PrivateApi("keep secret for now, only used in Metadata and it's not sure if we should re-use this")]
    public class VariableSource<TSource>: ICacheExpiring, ICacheDependent where TSource : class, ICacheExpiring
    {
        public DirectEntitiesSource SourceDirect { get; }
        public TSource SourceApp { get; }
        public Func<TSource> SourceDeferred { get; }

        public bool UseSource { get; set; } = true;

        public VariableSource(DirectEntitiesSource sourceDirect = default, TSource sourceApp = default, Func<TSource> sourceDeferred = default)
        {
            SourceDirect = sourceDirect;
            SourceApp = sourceApp;
            SourceDeferred = sourceDeferred;
        }

        public TSource MainSource => _mainSource.Get(() => SourceApp ?? SourceDeferred?.Invoke());
        private readonly GetOnce<TSource> _mainSource = new();

        public ICacheExpiring ExpirySource => _expirySourceReal.Get(() => (ICacheExpiring)SourceDirect ?? MainSource);
        private readonly GetOnce<ICacheExpiring> _expirySourceReal = new();

        /// <summary>
        /// The cache has a very "old" timestamp, so it's never newer than a dependent
        /// </summary>
        public long CacheTimestamp => ExpirySource?.CacheTimestamp ?? 0;

        public bool CacheChanged(long dependentTimeStamp) => ExpirySource?.CacheChanged(dependentTimeStamp) == true;

        public bool CacheChanged() => CacheChanged(CacheTimestamp);
    }
}
