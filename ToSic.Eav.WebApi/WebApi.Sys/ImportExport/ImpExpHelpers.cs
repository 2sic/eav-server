using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.Paths;
using ToSic.Eav.Context;
using ToSic.Eav.WebApi.Sys.Helpers.Http;
using ToSic.Eav.WebApi.Sys.Security;
using ToSic.Sys.Users;

namespace ToSic.Eav.WebApi.Sys.ImportExport;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ImpExpHelpers(IAppReaderFactory appReadFac, IUser user, ISite site, IAppPathsMicroSvc appPathSvc)
    : ServiceBase("Sxc.ImExHl", connect: [appReadFac, user, site, appPathSvc])
{
    /// <summary>
    /// Get an app - but only allow zone change if super-user
    /// </summary>
    /// <returns></returns>
    internal IAppReader GetReaderAfterZoneSwitchPermissionCheck(int appId) =>
        GetReaderAfterZoneSwitchPermissionCheck(site.ToAppIdentity(appId));

    /// <summary>
    /// Get an app - but only allow zone change if super-user
    /// </summary>
    /// <returns></returns>
    internal IAppReader GetReaderAfterZoneSwitchPermissionCheck(IAppIdentity appIdentity)
    {
        var l = Log.Fn<IAppReader>($"superuser: {user.IsSystemAdmin}; appIdentity: {appIdentity.Show()}");
        
        // Always do additional security checks, as some calls are opened as a new browser window
        // where API-attribute checks can't be done with certainty
        SecurityHelpers.ThrowIfNotSiteAdmin(user, Log);
        
        var contextZoneId = site.ZoneId;
        if (!user.IsSystemAdmin && appIdentity.ZoneId != contextZoneId)
        {
            l.ReturnNull("error");
            throw HttpException.PermissionDenied(
                "Tried to access app from another zone. Requires SuperUser permissions.");
        }

        var app = appReadFac.Get(appIdentity);
        return l.Return(app);
    }

    internal (IAppReader appReader, IAppPaths appPaths) GetReaderAndPathsAfterZoneSwitchPermissionCheck(IAppIdentity appIdentity)
    {
        var reader = GetReaderAfterZoneSwitchPermissionCheck(appIdentity);
        var appPaths = appPathSvc.Get(reader, site);
        return (reader, appPaths);
    }
}