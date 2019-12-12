using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps
{
    public class ZoneRuntime: ZoneBase
    {
        #region Constructor and simple properties

        public ZoneRuntime(int zoneId, ILog parentLog) : base(zoneId, parentLog, "App.Zone") {}

        #endregion


        public int DefaultAppId => Cache.Zones[ZoneId].DefaultAppId;

        public Dictionary<int, string> Apps => Cache.Zones[ZoneId].Apps;

        public string GetName(int appId) => Cache.Zones[ZoneId].Apps[appId];

        public List<DimensionDefinition> Languages(bool includeInactive = false) => includeInactive ? Cache.Zones[ZoneId].Languages : Cache.Zones[ZoneId].Languages.Where(l => l.Active).ToList();

    }
}
