using ToSic.Eav.Apps.ImportExport.Sys;
using ToSic.Eav.ImportExport.Sys;

namespace ToSic.Eav.WebApi.Admin.Sys;

public interface IAppController: IAppAdminWebApiController
{
    // UI test: Apps management > Apps list > delete an app.
    void App(int zoneId, int appId, bool fullDelete = true);

    // UI test: Apps management > Add App, or Create inherited app.
    void App(int zoneId, string name, int? inheritAppId = null);

    // UI test: Apps management > Apps list > Flush cache action for an app.
    bool FlushCache(int zoneId, int appId);

    // UI test: App administration > Import / Export > Export App > Download.
    THttpResponseType Export(int zoneId, int appId, bool includeContentGroups, bool resetAppGuid, bool assetsAdam, bool assetsSite, bool assetAdamDeleted);

    /// <summary>
    /// Read-only report of path casing risks before cross-platform migration.
    /// </summary>
    // UI test: Not exposed in eav-ui; exercise the admin/app/PathCasePreflight endpoint directly.
    PathCasePreflightResult PathCasePreflight(int zoneId, int appId);

    // UI test: App administration > Import / Export > App State > Save for version control.
    Task<bool> SaveData(int zoneId, int appId, bool includeContentGroups, bool resetAppGuid, bool withPortalFiles);


    /// <summary>
    /// Reset an App to the last xml state
    /// </summary>
    /// <returns></returns>
    // UI test: App administration > Import / Export > App State > Reset app.
    Task<ImportResult> Reset(int zoneId, int appId, bool withPortalFiles);

    // UI test: Apps management > Import App.
    ImportResult Import(int zoneId);

    // UI test: Apps management > Add Apps from Folder > install selected pending apps.
    ImportResult InstallPendingApps(int zoneId, IEnumerable<PendingAppRaw> pendingApps);
}
