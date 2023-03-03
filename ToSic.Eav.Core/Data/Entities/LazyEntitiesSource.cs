using System;
using System.Collections.Generic;
using ToSic.Eav.Caching;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Data
{
    public class LazyEntitiesSource<TSource>: ICacheExpiring, ICacheDependent where TSource : class, ICacheExpiring
    {
        private readonly ICacheExpiring _expirySource;
        public List<IEntity> SourceItems { get; }
        public TSource SourceApp { get; }
        public Func<TSource> SourceDeferred { get; }

        public bool UseSource { get; set; } = true;

        public LazyEntitiesSource(List<IEntity> items = default, ICacheExpiring expirySource = default, TSource sourceApp = default, Func<TSource> sourceDeferred = default)
        {
            SourceItems = items;
            _expirySource = expirySource;
            SourceApp = sourceApp;
            SourceDeferred = sourceDeferred;
        }

        public TSource MainSource => _mainSource.Get(() => SourceApp ?? SourceDeferred?.Invoke());
        private readonly GetOnce<TSource> _mainSource = new GetOnce<TSource>();

        public ICacheExpiring ExpirySource => _expirySourceReal.Get(() => _expirySource ?? MainSource);
        private readonly GetOnce<ICacheExpiring> _expirySourceReal = new GetOnce<ICacheExpiring>();

        //public List<IEntity> GetList()
        //{
        //    if (SourceItems != null) return SourceItems;
        //}

        public long CacheTimestamp { get; private set; }

        public bool CacheChanged(long newCacheTimeStamp) => newCacheTimeStamp < CacheTimestamp || CacheChanged();

        public bool CacheChanged() => SourceItems != null && ExpirySource?.CacheChanged(CacheTimestamp) == true;
    }
}
