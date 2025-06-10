using ToSic.Eav.WebApi.Sys.ApiExplorer;

namespace ToSic.Eav.WebApi.Sys.Admin;

public interface IAppExplorerControllerDependency
{
    ICollection<string> All(int appId, bool global, string path = null, string mask = "*.*", bool withSubfolders = false, bool returnFolders = false);

    ICollection<AllApiFileDto> AllApiFilesInAppCodeForAllEditions(int appId);

    //string GetEdition(int appId, string path);
}