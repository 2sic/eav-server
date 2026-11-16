namespace ToSic.Eav.WebApi.Sys.Install;

public interface IInstallController
{
    /// <summary>
    /// Finish system installation which had somehow been interrupted
    /// </summary>
    /// <returns></returns>
    bool Resume();

    /// <summary>
    /// Before this was GET Installer/InstallPackage
    /// </summary>
    /// <param name="packageUrl"></param>
    /// <param name="newName"></param>
    /// <returns></returns>
    THttpResponseType RemotePackage(string packageUrl, string? newName = null);
}