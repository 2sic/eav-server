using System;
using ToSic.Eav.Caching;

namespace ToSic.Eav.Apps
{
    public partial class AppState: ICacheExpiring
    {
        /// <inheritdoc />
        public int CacheUpdateCount { get; private set; }
        /// <inheritdoc />
        public long CacheTimestamp { get; private set; }

        private void CacheResetTimestamp()
        {
            CacheTimestamp = DateTime.Now.Ticks;
            CacheUpdateCount++;
            Log.Add($"cache reset #{CacheUpdateCount} to stamp {CacheTimestamp}");
        }

        /// <inheritdoc />
        public bool CacheChanged(long newCacheTimeStamp) => CacheTimestamp != newCacheTimeStamp;

    }
}
