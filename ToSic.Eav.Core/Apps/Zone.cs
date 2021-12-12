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
        public Zone(int zoneId, int primaryAppId, int contentAppId, Dictionary<int, string> apps, List<DimensionDefinition> languages)
        {
            ZoneId = zoneId;
            PrimaryAppId = primaryAppId;
            DefaultAppId = contentAppId;
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
        /// The Primary App which also contains Settings and shared Metadata
        /// WIP #SiteApp v13
        /// </summary>
        public int PrimaryAppId { get; }

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