using ToSic.Eav.WebApi.Admin.Sys;

namespace ToSic.Eav.WebApi.Sys.Install;

public interface IInstallController: IAppAdminWebApiController
{
    /// <summary>
    /// Finish system installation which had somehow been interrupted
    /// </summary>
    /// <returns></returns>
    // UI test: Not called by eav-ui; exercise sys/install/Resume only on an interrupted installation.
    bool Resume();

    /// <summary>
    /// Before this was GET Installer/InstallPackage
    /// </summary>
    /// <param name="packageUrl"></param>
    /// <param name="newName"></param>
    /// <returns></returns>
    // UI test: Apps management > Add App > install an app template/package from the catalog.
    THttpResponseType RemotePackage(string packageUrl, string? newName = null);
}
