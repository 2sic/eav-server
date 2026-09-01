using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.Admin;

public interface IAppExplorerControllerDependency
{
    ICollection<string> All(int appId, bool global, string? path = null, string mask = "*.*", bool withSubfolders = false, bool returnFolders = false);

    ICollection<AppWebApiFileRaw> AllApiFilesInAppCodeForAllEditions(int appId);
}
