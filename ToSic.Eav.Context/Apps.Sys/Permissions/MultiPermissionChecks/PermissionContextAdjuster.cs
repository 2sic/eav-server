using ToSic.Eav.Context;
using ToSic.Eav.Context.Sys.ZoneMapper;

namespace ToSic.Eav.Apps.Sys.Permissions;

/// <summary>
/// Helper to update a context to match the correct site - like when using modules across different sites.
/// </summary>
/// <param name="requestContext"></param>
/// <param name="zoneMapper"></param>
public class PermissionContextAdjuster(IContextOfSite requestContext, LazySvc<IZoneMapper> zoneMapper)
    : ServiceBase("Sec.ZoneSw", connect: [zoneMapper])
{
    /// <summary>
    /// Adjust the previous context to another site, if the site is different - for further security checks.
    /// </summary>
    /// <param name="ctxOfSiteOrMore"></param>
    /// <param name="app"></param>
    /// <returns></returns>
    public IContextOfSite AdjustContextForApp(IContextOfSite ctxOfSiteOrMore, IAppIdentity app)
    {
        var l = Log.Fn<IContextOfSite>($"AdjustContextForApp(app:{app.Show()})");

        // Figure out the site which should be used for the security check
        var siteForSecurityCheck = GetSiteForSecurityCheck(app);
        
        // user has edit permissions on this app, and it's the same app as the user is coming from
        var result = ctxOfSiteOrMore.WithSwappedSite(siteForSecurityCheck);

        return l.Return(result, $"adjusted context for app:{app.Show()} tenant/zone:{siteForSecurityCheck.Id}/{ctxOfSiteOrMore.Site.ZoneId}");
    }

    private ISite GetSiteForSecurityCheck(IAppIdentity app)
    {
        var context = requestContext;
        var isSameSite = context.Site.ZoneId == app.ZoneId;
        var l = Log.Fn($"..., appId: {app.AppId}, ...");

        var result = isSameSite
            ? context.Site
            // if the app is of another zone check that, but in multi-zone portals this won't find anything, so use current zone
            // todo: probably enhance with a Site.IsMultiZone check
            : zoneMapper.Value.SiteOfZone(app.ZoneId)
              ?? context.Site;
        l.Done($"ready for app:{app.Show()} tenant/zone:{result.Id}/{context.Site.ZoneId} same:{isSameSite}");
        return result;
    }
}