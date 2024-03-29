﻿namespace ToSic.Eav.WebApi.Sys;

public interface IInstallController<out THttpResponseType>
{
    /// <summary>
    /// Finish system installation which had somehow been interrupted
    /// </summary>
    /// <returns></returns>
    bool Resume();

    ///// <summary>
    ///// Before this was GET Module/RemoteInstallDialogUrl
    ///// </summary>
    ///// <param name="isContentApp"></param>
    ///// <returns></returns>
    //THttpResponseType RemoteWizardUrl(bool isContentApp);

    /// <summary>
    /// New API to get install URL etc. New in v15
    /// Requires latest build of quick dialog.
    /// </summary>
    /// <param name="isContentApp"></param>
    /// <returns></returns>
    InstallAppsDto InstallSettings(bool isContentApp);

    /// <summary>
    /// Before this was GET Installer/InstallPackage
    /// </summary>
    /// <param name="packageUrl"></param>
    /// <returns></returns>
    THttpResponseType RemotePackage(string packageUrl);
}