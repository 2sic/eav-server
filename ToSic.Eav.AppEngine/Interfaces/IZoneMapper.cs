using System.Collections.Generic;
using ToSic.Eav.Apps.Environment;

namespace ToSic.Eav.Apps.Interfaces
{
    public interface IZoneMapper
    {
        /// <summary>
        /// Get the zoneId which belongs to the tennant of this environment
        /// </summary>
        /// <param name="tennantId"></param>
        /// <returns></returns>
        int GetZoneId(int tennantId);

        int GetZoneId(ITennant tennant);


        ITennant Tennant(int zoneId);

        /// <summary>
        /// The cultures available on this tennant/zone combination
        /// the zone is necessary, to determine what is enabled/disabled
        /// </summary>
        /// <param name="tennantId"></param>
        /// <param name="zoneId"></param>
        /// <returns></returns>
        List<TempTempCulture> CulturesWithState(int tennantId, int zoneId);


    }
}
