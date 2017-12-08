using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps.Parts
{
    public class ZoneBase: IZoneIdentity
    {
        #region Constructor and simple properties
        public int ZoneId { get; }

        protected Log Log;

        public ZoneBase(int zoneId, Log parentLog, string logName)
        {
            ZoneId = zoneId;
            Log = new Log(logName, parentLog, $"zone base for z#{zoneId}");
        }

        internal BaseCache Cache => _cache ?? (_cache = (BaseCache)DataSource.GetCache(ZoneId));
        private BaseCache _cache;

        #endregion

        
    }
}
