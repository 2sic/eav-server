namespace ToSic.Eav.WebApi.Sys.ApiExplorer;

public interface IApiExplorerController
{
    THttpResponseType Inspect(string path);

    AllApiFilesDto AppApiFiles(int appId);
}