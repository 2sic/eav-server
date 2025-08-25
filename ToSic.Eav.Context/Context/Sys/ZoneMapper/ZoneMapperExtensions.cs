using ToSic.Eav.Apps;
using ToSic.Eav.Sys;

namespace ToSic.Eav.Context.Sys.ZoneMapper;

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
    /// <param name="appIdentity"></param>
    /// <returns></returns>
    internal static ISite SiteOfAppIfSiteInvalid(this LazySvc<IZoneMapper> zoneMapperLazy, ISite siteFromDi, IAppIdentity appIdentity)
    {
        if (siteFromDi.Id != EavConstants.NullId && siteFromDi.ZoneId == appIdentity.ZoneId)
            return siteFromDi;

        var zoneMapper = zoneMapperLazy.Value;
        var l = zoneMapper.Log.Fn<ISite>($"Trying to build path based on tenant.");
        try
        {
            //if (siteFromDi.Id != Eav.Constants.NullId)
            //    return l.Return(siteFromDi, $"All ok since siteId isn't {Eav.Constants.NullId}");

            if (siteFromDi.Id == EavConstants.NullId)
                l.A($"SiteId = {siteFromDi.Id} - not found. Must be in search mode as DI failed, will try to find correct PortalSettings");
            else if(siteFromDi.ZoneId != appIdentity.ZoneId) 
                l.A($"SiteId = {siteFromDi.Id} - has different di-site-ZoneId:{siteFromDi.ZoneId} from app-ZoneID:{appIdentity.ZoneId}. Must be app is first accessed from another site. Will try to find SiteOfApp");

            var correctedSite = zoneMapper.SiteOfApp(appIdentity.AppId);
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