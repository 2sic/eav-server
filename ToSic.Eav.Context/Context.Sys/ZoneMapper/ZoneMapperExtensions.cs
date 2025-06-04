using ToSic.Eav.Context;
using ToSic.Eav.Sys;

namespace ToSic.Eav.Integration;

public static class ZoneMapperExtensions
{
    /// <summary>
    /// Return a site. Either the one provided (if it's valid)
    /// or get the one belonging to the app.
    ///
    /// This is primarily meant for scenarios where the injected site is not valid,
    /// e.g. in background tasks such as search.
    /// </summary>
    /// <param name="siteFromDi"></param>
    /// <param name="appId"></param>
    /// <returns></returns>
    internal static ISite SiteOfAppIfSiteInvalid(this LazySvc<IZoneMapper> zoneMapperLazy, ISite siteFromDi, int appId)
    {
        if (siteFromDi.Id != EavConstants.NullId)
            return siteFromDi;

        var zoneMapper = zoneMapperLazy.Value;
        var l = zoneMapper.Log.Fn<ISite>($"Trying to build path based on tenant. If it's in search mode, the {nameof(ISite)} would be {EavConstants.NullId}. Id: {siteFromDi.Id}");
        try
        {
            //if (siteFromDi.Id != Eav.Constants.NullId)
            //    return l.Return(siteFromDi, $"All ok since siteId isn't {Eav.Constants.NullId}");

            l.A($"SiteId = {siteFromDi.Id} - not found. Must be in search mode as DI failed, will try to find correct PortalSettings");
            var correctedSite = zoneMapper.SiteOfApp(appId);
            return l.Return(correctedSite, $"SiteId: {correctedSite.Id}");
        }
        catch (Exception e)
        {
            // ignore
            l.Ex(e);
            return l.Return(siteFromDi, "error");
        }
    }

}