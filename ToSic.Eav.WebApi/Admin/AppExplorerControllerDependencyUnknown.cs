﻿using ToSic.Eav.Internal.Unknown;
using ToSic.Eav.WebApi.ApiExplorer;

namespace ToSic.Eav.WebApi.Admin;

public class AppExplorerControllerDependencyUnknown : ServiceBase, IAppExplorerControllerDependency
{
    public AppExplorerControllerDependencyUnknown(WarnUseOfUnknown<AppExplorerControllerDependencyUnknown> _) : base($"{LogScopes.NotImplemented}.AdmFleCtrl") { }

    public List<string> All(int appId, bool global, string path = null, string mask = "*.*", bool withSubfolders = false, bool returnFolders = false)
    {
        throw new NotImplementedException();
    }

    public List<AllApiFileDto> AllApiFilesInAppCodeForAllEditions(int appId)
    {
        throw new NotImplementedException();
    }

    //public string GetEdition(int appId, string path)
    //{
    //    throw new NotImplementedException();
    //}
}