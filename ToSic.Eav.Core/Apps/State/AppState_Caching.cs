using System;
using ToSic.Eav.Caching;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    public partial class AppState: ICacheExpiring, ICacheExpiringDelegated
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

            CacheStatistics.Update(CacheTimestamp, Index.Count, message);
            Log.Add($"cache reset to stamp {CacheTimestamp} = {CacheTimestamp.ToReadable()}");
            Log.Add($"Stats: ItemCount: {Index.Count}; ResetCount: {CacheStatistics.ResetCount}  Message: '{message}'");
        }


        /// <inheritdoc />
        public bool CacheChanged(long newCacheTimeStamp) => CacheTimestamp != newCacheTimeStamp;

        /// <summary>
        /// The App can itself be the master of expiry, or it can be that a parent-app must be included
        /// So the expiry-provider is this object, which must be initialized on AppState creation
        /// </summary>
        [PrivateApi]
        public ICacheExpiring CacheExpiryDelegate { get; }

        private ICacheExpiring CreateExpiryProvider()
        {
            // todo: check if feature is enabled #SharedAppFeatureEnabled
            return ParentApp.InheritEntities && ParentApp.AppState != null
                ? new CacheExpiringMultiSource(this, ParentApp.AppState)
                : this as ICacheExpiring;
        }
    }
}
