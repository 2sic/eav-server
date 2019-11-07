using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps
{
    public class ZoneRuntime: ZoneBase
    {
        #region Constructor and simple properties

        public ZoneRuntime(int zoneId, ILog parentLog) : base(zoneId, parentLog, "App.Zone") {}

        #endregion


        public int DefaultAppId => Cache.ZoneApps[ZoneId].DefaultAppId;

        public Dictionary<int, string> Apps => Cache.ZoneApps[ZoneId].Apps;

        public string GetName(int appId) => Cache.ZoneApps[ZoneId].Apps[appId];

        public List<DimensionDefinition> Languages(bool includeInactive = false) => includeInactive ? Cache.ZoneApps[ZoneId].Languages : Cache.ZoneApps[ZoneId].Languages.Where(l => l.Active).ToList();

    }
}
