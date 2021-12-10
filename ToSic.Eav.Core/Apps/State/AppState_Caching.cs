using System;
using ToSic.Eav.Caching;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    public partial class AppState: ICacheExpiring
    {
        /// <summary>
        /// Helper object to keep track of cache changes
        /// </summary>
        [PrivateApi] public ICacheStatistics CacheStatistics = new CacheStatistics();
        
        /// <inheritdoc />
        public long CacheTimestamp { get; private set; }

        private void CacheResetTimestamp(string message, int offset = 0)
        {
            // Update time stamp
            // In very rare, fast cases the timestamp is unmodified
            // In such cases we must make sure it's incremented by at least 1
            var prevTimeStamp = CacheTimestamp;
            CacheTimestamp = DateTime.Now.Ticks + offset;
            if (prevTimeStamp == CacheTimestamp) CacheTimestamp++;

            CacheStatistics.Update(CacheTimestamp, Index?.Count ?? 0, message);
            Log.Add($"cache reset to stamp {CacheTimestamp} = {CacheTimestamp.ToReadable()}");
            Log.Add($"Stats: ItemCount: {Index?.Count}; ResetCount: {CacheStatistics.ResetCount}  Message: '{message}'");
        }


        /// <inheritdoc />
        public bool CacheChanged(long newCacheTimeStamp) => CacheTimestamp != newCacheTimeStamp;

    }
}
