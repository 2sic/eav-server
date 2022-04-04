using System;
using ToSic.Eav.Caching;
using ToSic.Eav.Data.PiggyBack;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    public partial class AppState: ICacheExpiring, IHasPiggyBack
    {
        /// <summary>
        /// Helper object to keep track of cache changes
        /// </summary>
        [PrivateApi] public ICacheStatistics CacheStatistics = new CacheStatistics();

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
            Log.Add($"cache reset to stamp {CacheTimestamp} = {CacheTimestamp.ToReadable()}");
            Log.Add($"Stats: ItemCount: {Index.Count}; ResetCount: {CacheStatistics.ResetCount}  Message: '{message}'");

            RaiseAppStateChanged(new AppStateChangedEventArgs(AppId)); // so lightspeed can flush cache
        }


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


        // Custom event for LightSpeed
        public event EventHandler<AppStateChangedEventArgs> AppStateChanged;

        protected virtual void RaiseAppStateChanged(AppStateChangedEventArgs e) => AppStateChanged?.Invoke(this, new AppStateChangedEventArgs(AppId));
    }

    // Define a class to hold custom event info
    public class AppStateChangedEventArgs : EventArgs
    {
        public AppStateChangedEventArgs(int appId) => AppId = appId;

        public int AppId { get; set; }
    }
}
