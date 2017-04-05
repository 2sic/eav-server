using ToSic.Eav.DataSources.Caches;

namespace ToSic.Eav.Apps.Interfaces
{
    public class ZoneBase: IZone
    {
        #region Constructor and simple properties
        public int ZoneId { get; }

        public ZoneBase(int zoneId)
        {
            ZoneId = zoneId;
        }

        internal BaseCache Cache => _cache ?? (_cache = (BaseCache)DataSource.GetCache(ZoneId));
        private BaseCache _cache;

        #endregion

        
    }
}
