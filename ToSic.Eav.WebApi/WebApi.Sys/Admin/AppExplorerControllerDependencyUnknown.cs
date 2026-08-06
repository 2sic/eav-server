using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.Admin;

public class AppExplorerControllerDependencyUnknown : ServiceBase, IAppExplorerControllerDependency
{
    public AppExplorerControllerDependencyUnknown(WarnUseOfUnknown<AppExplorerControllerDependencyUnknown> _) : base($"{LogScopes.NotImplemented}.AdmFleCtrl") { }

    public ICollection<string> All(int appId, bool global, string? path = null, string mask = "*.*", bool withSubfolders = false, bool returnFolders = false)
    {
        throw new NotImplementedException();
    }

    public ICollection<AppWebApiFileRaw> AllApiFilesInAppCodeForAllEditions(int appId)
    {
        throw new NotImplementedException();
    }
}
