using System;
using ToSic.Eav.Caching;

namespace ToSic.Eav.Apps
{
    public partial class AppState: ICacheExpiring
    {
        public int CacheUpdateCount { get; private set; }
        public long CacheTimestamp { get; private set; }

        public void CacheResetTimestamp()
        {
            CacheTimestamp = DateTime.Now.Ticks;
            CacheUpdateCount++;
            Log.Add($"cache reset #{CacheUpdateCount} to stamp {CacheTimestamp}");
        }

        public bool CacheChanged(long newCacheTimeStamp) => CacheTimestamp != newCacheTimeStamp;

    }
}
