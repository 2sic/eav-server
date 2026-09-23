using ToSic.Eav.ImportExport.Sys;
using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.Admin;

public interface IAppController
{
    void App(int zoneId, int appId, bool fullDelete = true);

    void App(int zoneId, string name, int? inheritAppId = null);

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


    /// <summary>
    /// Reset an App to the last xml state
    /// </summary>
    /// <returns></returns>
    Task<ImportResultDto> Reset(int zoneId, int appId, bool withPortalFiles);

    ImportResultDto Import(int zoneId);


    ImportResultDto InstallPendingApps(int zoneId, IEnumerable<PendingAppDto> pendingApps);
}
