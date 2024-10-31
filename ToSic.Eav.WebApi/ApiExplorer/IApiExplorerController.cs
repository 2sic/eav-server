#if NETFRAMEWORK
using THttpResponseType = System.Net.Http.HttpResponseMessage;
#else
using THttpResponseType = Microsoft.AspNetCore.Mvc.IActionResult;
#endif

namespace ToSic.Eav.WebApi.ApiExplorer;

public interface IApiExplorerController
{
    THttpResponseType Inspect(string path);

    AllApiFilesDto AppApiFiles(int appId);
}