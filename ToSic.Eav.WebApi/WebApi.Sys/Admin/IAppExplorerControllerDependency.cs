using ToSic.Eav.WebApi.Sys.ApiExplorer;

namespace ToSic.Eav.WebApi.Sys.Admin;

public interface IAppExplorerControllerDependency
{
    List<string> All(int appId, bool global, string path = null, string mask = "*.*", bool withSubfolders = false, bool returnFolders = false);

    List<AllApiFileDto> AllApiFilesInAppCodeForAllEditions(int appId);

    //string GetEdition(int appId, string path);
}