using ToSic.Eav.Context;
using ToSic.Eav.Context.Sys.ZoneMapper;

namespace ToSic.Eav.Apps.Sys.Permissions;

public class ZoneSwitchPermissionCheck(IContextOfSite requestContext, LazySvc<IZoneMapper> zoneMapper)
    : ServiceBase("Sec.ZoneSw", connect: [zoneMapper])
{
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