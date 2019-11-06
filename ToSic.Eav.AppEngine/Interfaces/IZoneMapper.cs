using System.Collections.Generic;
using ToSic.Eav.Apps.Environment;

namespace ToSic.Eav.Apps
{
    public interface IZoneMapper
    {
        /// <summary>
        /// Get the zoneId which belongs to the tenant of this environment
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        int GetZoneId(int tenantId);

        int GetZoneId(ITenant tenant);


        ITenant Tenant(int zoneId);

        /// <summary>
        /// The cultures available on this tenant/zone combination
        /// the zone is necessary, to determine what is enabled/disabled
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="zoneId"></param>
        /// <returns></returns>
        List<TempTempCulture> CulturesWithState(int tenantId, int zoneId);


    }
}
