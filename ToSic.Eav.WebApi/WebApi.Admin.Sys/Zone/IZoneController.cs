namespace ToSic.Eav.WebApi.Admin.Sys;

/// <summary>
/// This one supplies portal-wide (or cross-portal) settings / configuration
/// </summary>
public interface IZoneController: IAppAdminWebApiController
{
    /// <summary>
    /// Enable / disable a language in the EAV
    /// </summary>
    /// <returns></returns>
    // UI test: Apps management > Site Languages > enable or disable a language.
    void SwitchLanguage(string cultureCode, bool enable);

}
