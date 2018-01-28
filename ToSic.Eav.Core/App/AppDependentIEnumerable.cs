using System;
using System.Collections;
using System.Collections.Generic;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.App
{
    public class AppDependentIEnumerable<T>: IEnumerable<T>, ICacheDependent
    {
        protected readonly AppDataPackage App;
        private List<T> _cache;
        private readonly Func</*AppDataPackage,*/ List<T>> _rebuild;

        public AppDependentIEnumerable(AppDataPackage app, Func</*AppDataPackage, */List<T>> rebuild)
        {
            App = app;
            _rebuild = rebuild;
        }


        public List<T> GetCache()
        {
             if (_cache != null && !CacheChanged()) return _cache;

            _cache = _rebuild.Invoke(/*App*/);
            CacheTimestamp = App.CacheTimestamp;

            return _cache;
        }

        public IEnumerator<T> GetEnumerator() 
            => GetCache().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public long CacheTimestamp { get; private set; }
        public bool CacheChanged() => App.CacheChanged(CacheTimestamp);
    }
}
