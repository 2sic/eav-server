using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.DataSources.Caching;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps.Parts
{
    public class ZoneBase: IInZone
    {
        #region Constructor and simple properties
        public int ZoneId { get; }

        protected ILog Log;

        public ZoneBase(int zoneId, ILog parentLog, string logName)
        {
            ZoneId = zoneId;
            Log = new Log(logName, parentLog, $"zone base for z#{zoneId}");
        }

        internal RootCache Cache => _cache ?? (_cache = (RootCache)DataSource.GetCache(ZoneId));
        private RootCache _cache;

        #endregion

        
    }
}
