namespace ToSic.Eav.WebApi.Sys.Admin;

/// <summary>
/// This one supplies portal-wide (or cross-portal) settings / configuration
/// </summary>
public interface IZoneController
{
    /// <summary>
    /// Enable / disable a language in the EAV
    /// </summary>
    /// <returns></returns>
    void SwitchLanguage(string cultureCode, bool enable);

}
