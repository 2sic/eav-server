using System.Collections.Generic;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents a Zone
    /// </summary>
    public class Zone
    {
        public Zone(int zoneId, int defAppId, Dictionary<int, string> apps, List<DimensionDefinition> languages)
        {
            ZoneId = zoneId;
            DefaultAppId = defAppId;
            Apps = apps;
            Languages = languages;
        }

        /// <summary>
        /// ZoneId
        /// </summary>
        public int ZoneId { get; internal set; }
        /// <summary>
        /// AppId of the default App in this Zone
        /// </summary>
        public int DefaultAppId { get; }
        /// <summary>
        /// All Apps in this Zone with Id and Name
        /// </summary>
        public Dictionary<int, string> Apps { get; internal set; }

        public List<DimensionDefinition> Languages { get; }

    }
}