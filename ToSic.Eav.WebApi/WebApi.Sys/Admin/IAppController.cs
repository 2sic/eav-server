using ToSic.Eav.ImportExport.Sys;
using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.Admin;

public interface IAppController
{
    // Replaced by DataSource System.Apps
    //ICollection<AppDto> List(int zoneId);

    // Replaced by DataSource System.InheritableApps
    //ICollection<AppDto> InheritableApps();

    void App(int zoneId, int appId, bool fullDelete = true);

    void App(int zoneId, string name, int? inheritAppId = null);

    // Replaced by DataSource System.AppLanguages
    //ICollection<SiteLanguageDto> Languages(int appId);

    // Replaced by DataSource System.AppStatistics through query System.SysData.
    // Use app/auto/query/System.SysData/Default with SysDataSource=System.AppStatistics.
    //AppExportInfoDto Statistics(int zoneId, int appId);

    bool FlushCache(int zoneId, int appId);

    THttpResponseType Export(int zoneId, int appId, bool includeContentGroups, bool resetAppGuid, bool assetsAdam, bool assetsSite, bool assetAdamDeleted);

    /// <summary>
    /// Read-only report of path casing risks before cross-platform migration.
    /// </summary>
    PathCasePreflightResult PathCasePreflight(int zoneId, int appId);

    Task<bool> SaveData(int zoneId, int appId, bool includeContentGroups, bool resetAppGuid, bool withPortalFiles);

    // Replaced by DataSource System.SystemStack through query System.SysData.
    // Use app/auto/query/System.SysData/Default with SysDataSource=System.SystemStack.
    ///// <summary>
    ///// Get a stack of values from settings or resources
    ///// </summary>
    ///// <param name="appId"></param>
    ///// <param name="part">Name of the part - "settings" or "resources"</param>
    ///// <param name="key">Optional key like "Settings.Images.Content.Width"</param>
    ///// <param name="view">Optional guid of a view to merge with the settings</param>
    ///// <returns></returns>
    //List<AppStackDataRaw> GetStack(int appId, string part, string? key = null, Guid? view = null);

    /// <summary>
    /// Reset an App to the last xml state
    /// </summary>
    /// <returns></returns>
    Task<ImportResultDto> Reset(int zoneId, int appId, bool withPortalFiles);

    ImportResultDto Import(int zoneId);

    /// <summary>
    /// Install pending apps
    /// </summary>
    /// <param name="zoneId"></param>
    /// <param name="pendingApps"></param>
    /// <returns></returns>
    // Replaced by DataSource System.AppsPendingInitialization through query System.SysData.
    //IEnumerable<PendingAppDto> GetPendingApps(int zoneId);

    ImportResultDto InstallPendingApps(int zoneId, IEnumerable<PendingAppDto> pendingApps);
}
