using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Context;
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
        int GetZoneId(int siteId);

        /// <summary>
        /// Get the zoneId which belongs to the tenant of this environment
        /// </summary>
        int GetZoneId(ISite site);

        ///// <summary>
        ///// Get the App Identity when we know the tenant and app-id
        ///// </summary>
        //IAppIdentity IdentityFromSite(int tenantId, int appId);


        /// <summary>
        /// Find the tenant of a Zone
        /// </summary>
        ISite SiteOfZone(int zoneId);

        /// <summary>
        /// Find the tenant of an App
        /// </summary>
        ISite TenantOfApp(int appId);

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
