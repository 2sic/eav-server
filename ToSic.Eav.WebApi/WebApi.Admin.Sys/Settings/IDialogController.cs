namespace ToSic.Eav.WebApi.Admin.Sys;

public interface IDialogController: IAppAdminWebApiController
{
    /// <summary>
    /// This is the subsystem which delivers the getting-started app-iframe with instructions etc.
    /// Used to be GET System/DialogSettings
    /// </summary>
    /// <param name="appId"></param>
    /// <returns></returns>
    // UI test: Open any eav-ui dialog or administration screen; its startup loads these settings.
    DialogContextStandaloneDto Settings(int appId);
}
