using System;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.App
{
    public partial class AppDataPackage: ICacheExpiring
    {
        public long CacheTimestamp { get; private set; }

        public void CacheResetTimestamp() => CacheTimestamp = DateTime.Now.Ticks;

        public bool CacheChanged(long prevCacheTimestamp) => CacheTimestamp != prevCacheTimestamp;

    }
}
