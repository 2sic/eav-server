namespace ToSic.Eav.WebApi.Sys
{
    public interface IInstallController<THttpResponseType>
    {
        /// <summary>
        /// Finish system installation which had somehow been interrupted
        /// </summary>
        /// <returns></returns>
        bool Resume();

        /// <summary>
        /// Before this was GET Module/RemoteInstallDialogUrl
        /// </summary>
        /// <param name="isContentApp"></param>
        /// <returns></returns>
        THttpResponseType RemoteWizardUrl(bool isContentApp);

        /// <summary>
        /// Before this was GET Installer/InstallPackage
        /// </summary>
        /// <param name="packageUrl"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        THttpResponseType RemotePackage(string packageUrl);
    }
}