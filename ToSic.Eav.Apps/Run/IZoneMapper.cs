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
        int GetZoneId(int tenantId);

        /// <summary>
        /// Get the zoneId which belongs to the tenant of this environment
        /// </summary>
        int GetZoneId(ITenant tenant);

        /// <summary>
        /// Get the App Identity when we know the tenant and app-id
        /// </summary>
        IAppIdentity IdentityFromTenant(int tenantId, int appId);


        /// <summary>
        /// Find the tenant of a Zone
        /// </summary>
        ITenant TenantOfZone(int zoneId);

        /// <summary>
        /// Find the tenant of an App
        /// </summary>
        ITenant TenantOfApp(int appId);

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
