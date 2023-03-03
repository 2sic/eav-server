using System;
using System.Collections.Generic;
using ToSic.Eav.Caching;
using ToSic.Eav.Data.LazySource;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Data
{
    public class LazyEntitiesSource<TSource>: ICacheExpiring, ICacheDependent where TSource : class, ICacheExpiring
    {
        public UnLazyEntities UnLazySource { get; }
        public List<IEntity> SourceItems { get; }
        public TSource SourceApp { get; }
        public Func<TSource> SourceDeferred { get; }

        public bool UseSource { get; set; } = true;

        public LazyEntitiesSource(UnLazyEntities unLazySource = default, TSource sourceApp = default, Func<TSource> sourceDeferred = default)
        {
            UnLazySource = unLazySource;
            SourceApp = sourceApp;
            SourceDeferred = sourceDeferred;
        }

        public TSource MainSource => _mainSource.Get(() => SourceApp ?? SourceDeferred?.Invoke());
        private readonly GetOnce<TSource> _mainSource = new GetOnce<TSource>();

        public ICacheExpiring ExpirySource => _expirySourceReal.Get(() => UnLazySource as ICacheExpiring ?? MainSource);
        private readonly GetOnce<ICacheExpiring> _expirySourceReal = new GetOnce<ICacheExpiring>();

        //public List<IEntity> GetList()
        //{
        //    if (SourceItems != null) return SourceItems;
        //}

        /// <summary>
        /// The cache has a very "old" timestamp, so it's never newer than a dependent
        /// </summary>
        public long CacheTimestamp => 0;

        public bool CacheChanged(long dependentTimeStamp) => dependentTimeStamp < CacheTimestamp || CacheChanged();

        public bool CacheChanged() => SourceItems != null && ExpirySource?.CacheChanged(CacheTimestamp) == true;
    }
}
