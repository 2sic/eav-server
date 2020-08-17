using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Run
{
    [PrivateApi]
    public interface IZoneMapper: IHasLog<IZoneMapper>
    {
        /// <summary>
        /// Get the zoneId which belongs to the tenant of this environment
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        int GetZoneId(int tenantId);

        int GetZoneId(ITenant tenant);

        IAppIdentity IdentityFromTenant(int tenantId, int appId);


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
