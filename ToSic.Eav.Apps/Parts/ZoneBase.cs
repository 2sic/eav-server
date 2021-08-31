using ToSic.Eav.Caching;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Parts
{
    public class ZoneBase<T>: HasLog, IZoneIdentity where T: ZoneBase<T>
    {
        #region Constructor and simple properties
        public int ZoneId { get; private set; }

        public ZoneBase(string logName): base(logName) { }

        public T Init(int zoneId, ILog parentLog)
        {
            Log.LinkTo(parentLog);
            ZoneId = zoneId;
            Log.Add($"zone base for z#{zoneId}");
            return this as T;
        }



        //internal IAppsCache Cache => _cache ?? (_cache = State.Cache);
        //private IAppsCache _cache;

        #endregion

        
    }
}
