using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// Contains all the basic infos about a Zone - usually cached
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public class Zone: IZoneIdentity
    {
        public Zone(int zoneId, int defAppId, Dictionary<int, string> apps, List<DimensionDefinition> languages)
        {
            ZoneId = zoneId;
            DefaultAppId = defAppId;
            Apps = apps;
            Languages = languages;
        }

        /// <inheritdoc />
        public int ZoneId { get; internal set; }

        /// <summary>
        /// AppId of the default App in this Zone
        /// </summary>
        public int DefaultAppId { get; }

        /// <summary>
        /// All Apps in this Zone with Id and Name
        /// </summary>
        public Dictionary<int, string> Apps { get; internal set; }

        /// <summary>
        /// Languages available in this Zone
        /// </summary>
        public List<DimensionDefinition> Languages { get; }

    }
}