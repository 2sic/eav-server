using ToSic.Eav.Context;
using ToSic.Eav.Context.Sys.ZoneMapper;

namespace ToSic.Eav.Apps.Sys.Permissions;

public class ZoneSwitchPermissionCheck(IContextOfSite requestContext, LazySvc<IZoneMapper> zoneMapper)
    : ServiceBase("Sec.ZoneSw", connect: [zoneMapper])
{
    //public ISite GetSiteForSecurityCheck(IAppIdentity app)
    //{
    //    var context = requestContext;
    //    var isSameSite = context.Site.ZoneId == app.ZoneId;
    //    var l = Log.Fn($"..., appId: {app.AppId}, ...");

    //    var result = isSameSite
    //        ? context.Site
    //        // if the app is of another zone check that, but in multi-zone portals this won't find anything, so use current zone
    //        // todo: probably enhance with a Site.IsMultiZone check
    //        : zoneMapper.Value.SiteOfZone(app.ZoneId)
    //          ?? context.Site;
    //    l.Done($"ready for app:{app.Show()} tenant/zone:{result.Id}/{context.Site.ZoneId} same:{isSameSite}");
    //    return result;
    //}


    /// <summary>
    /// Test if we are switching zones - and if yes, if it is allowed (superuser only)
    /// </summary>
    /// <param name="app"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public bool Check(IAppIdentity app, out string? error)
    {
        var l = Log.Fn<bool>();
        var isSameSite = requestContext.Site.ZoneId == app.ZoneId;

        var zoneSameOrSuperUser = isSameSite || requestContext.User.IsSystemAdmin;
        error = zoneSameOrSuperUser
            ? null
            : $"accessing app {app.Show()} is not allowed for this user as it changes zones";
        return l.Return(zoneSameOrSuperUser, zoneSameOrSuperUser ? $"SamePortal:{isSameSite} - ok" : "not ok, generate error");
    }
}

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

    public ISite GetSiteForSecurityCheck(IAppIdentity app)
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

