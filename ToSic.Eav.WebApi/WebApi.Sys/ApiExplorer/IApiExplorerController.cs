namespace ToSic.Eav.WebApi.Sys.ApiExplorer;

public interface IApiExplorerController
{
    THttpResponseType Inspect(string path);

    // 2rb: 2026-06-12: Replaced by AppWebApiControllers Datasource
    //AllApiFilesDto AppApiFiles(int appId);
}