using ToSic.Eav.Caching.Apps;
using ToSic.Eav.DataSources.Caching;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps.Parts
{
    public class ZoneBase: IZoneIdentity
    {
        #region Constructor and simple properties
        public int ZoneId { get; }

        protected ILog Log;

        public ZoneBase(int zoneId, ILog parentLog, string logName)
        {
            ZoneId = zoneId;
            Log = new Log(logName, parentLog, $"zone base for z#{zoneId}");
        }

        internal IAppsCache Cache => _cache ?? (_cache = Factory.GetAppsCache());//(RootCacheBase)DataSource.GetCache(ZoneId));
        private IAppsCache _cache;

        #endregion

        
    }
}
