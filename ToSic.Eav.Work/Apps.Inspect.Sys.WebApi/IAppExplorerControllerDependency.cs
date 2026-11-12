using ToSic.Eav.Apps.Inspect.Sys.WebApi.Models;

namespace ToSic.Eav.Apps.Inspect.Sys.WebApi;

public interface IAppExplorerControllerDependency
{
    ICollection<string> All(int appId, bool global, string? path = null, string mask = "*.*", bool withSubfolders = false, bool returnFolders = false);

    ICollection<AppWebApiFileRaw> AllApiFilesInAppCodeForAllEditions(int appId);
}
