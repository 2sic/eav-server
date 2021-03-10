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

        private void CacheResetTimestamp()
        {
            CacheTimestamp = DateTime.Now.Ticks;
            CacheStatistics.Update(CacheTimestamp);
            Log.Add($"cache reset #{CacheStatistics.ResetCount} to stamp {CacheTimestamp} = {CacheTimestamp.ToReadable()}");
        }


        /// <inheritdoc />
        public bool CacheChanged(long newCacheTimeStamp) => CacheTimestamp != newCacheTimeStamp;

    }
}
