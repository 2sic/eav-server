using System;
using ToSic.Eav.Caching;
using ToSic.Eav.Data.PiggyBack;
using ToSic.Lib.Logging;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Apps
{
    public partial class AppState: ICacheExpiring, IHasPiggyBack
    {
        /// <summary>
        /// Helper object to keep track of cache changes
        /// </summary>
        [PrivateApi] public ICacheStatistics CacheStatistics = new CacheStatistics();


        // Custom event for LightSpeed
        [PrivateApi] public event EventHandler AppStateChanged;

        /// <inheritdoc />
        public long CacheTimestamp => CacheExpiryDelegate.CacheTimestamp;

        private void CacheResetTimestamp(string message, int offset = 0)
        {
            // Update time stamp
            // In very rare, fast cases the timestamp is unmodified
            // In such cases we must make sure it's incremented by at least 1
            var prevTimeStamp = CacheTimestamp;
            CacheTimestampPrivate.CacheTimestamp = DateTime.Now.Ticks + offset;
            if (prevTimeStamp == CacheTimestampPrivate.CacheTimestamp)
                CacheTimestampPrivate.CacheTimestamp++;

            CacheStatistics.Update(CacheTimestamp, Index.Count, message);
            Log.A($"cache reset to stamp {CacheTimestamp} = {CacheTimestamp.ToReadable()}");
            Log.A($"Stats: ItemCount: {Index.Count}; ResetCount: {CacheStatistics.ResetCount}  Message: '{message}'");

            AppStateChanged?.Invoke(this, EventArgs.Empty); // publish event so lightspeed can flush cache
        }

        /// <summary>
        /// Call this method before AppState object is destroyed and recreate (new object will get new reference)
        /// to ensure that dependent object are notified.  
        /// </summary>
        // IMPORTANT: This is called by the farm cache, which is not part of this solution. That's why you don't have any access-counts
        [PrivateApi] public void PreRemove() => CacheResetTimestamp("AppState object will be destroyed and recreated as new object", 1);


        /// <inheritdoc />
        public bool CacheChanged(long newCacheTimeStamp) => CacheTimestamp != newCacheTimeStamp;

        /// <summary>
        /// The App can itself be the master of expiry, or it can be that a parent-app must be included
        /// So the expiry-provider is this object, which must be initialized on AppState creation
        /// </summary>
        private ITimestamped CacheExpiryDelegate { get; }

        /// <summary>
        /// Store for the app-private timestamp. In inherited apps, it will be combined with the parent using the CacheExpiryDelegate
        /// </summary>
        private Timestamped CacheTimestampPrivate { get; } = new Timestamped();

        /// <summary>
        /// Create an expiry source for this app.
        /// In normal mode it will only use the private timestamp.
        /// In shared mode it will merge its timestamp with the parent
        /// </summary>
        private ITimestamped CreateExpiryDelegate(ParentAppState pApp)
            => (pApp.InheritContentTypes || pApp.InheritEntities) && pApp.AppState != null
                ? new CacheExpiringMultiSource(CacheTimestampPrivate, pApp.AppState) as ITimestamped
                : CacheTimestampPrivate;

        [PrivateApi] 
        public PiggyBack PiggyBack => _piggyBack ?? (_piggyBack = new PiggyBack());
        private PiggyBack _piggyBack;

    }
}
