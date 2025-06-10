using ToSic.Eav.WebApi.Sys.ApiExplorer;

namespace ToSic.Eav.WebApi.Sys.Admin;

public class AppExplorerControllerDependencyUnknown : ServiceBase, IAppExplorerControllerDependency
{
    public AppExplorerControllerDependencyUnknown(WarnUseOfUnknown<AppExplorerControllerDependencyUnknown> _) : base($"{LogScopes.NotImplemented}.AdmFleCtrl") { }

    public ICollection<string> All(int appId, bool global, string path = null, string mask = "*.*", bool withSubfolders = false, bool returnFolders = false)
    {
        throw new NotImplementedException();
    }

    public ICollection<AllApiFileDto> AllApiFilesInAppCodeForAllEditions(int appId)
    {
        throw new NotImplementedException();
    }

    //public string GetEdition(int appId, string path)
    //{
    //    throw new NotImplementedException();
    //}
}