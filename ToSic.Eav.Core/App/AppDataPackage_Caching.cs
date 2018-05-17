using System;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.App
{
    public partial class AppDataPackage: ICacheExpiring
    {
        public int CacheUpdateCount { get; private set; }
        public long CacheTimestamp { get; private set; }

        public void CacheResetTimestamp()
        {
            CacheTimestamp = DateTime.Now.Ticks;
            CacheUpdateCount++;
            Log.Add($"cache reset #{CacheUpdateCount} to stamp {CacheTimestamp}");
        }

        public bool CacheChanged(long prevCacheTimestamp) => CacheTimestamp != prevCacheTimestamp;

    }
}
