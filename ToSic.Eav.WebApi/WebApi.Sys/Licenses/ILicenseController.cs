using ToSic.Eav.WebApi.Admin.Sys;

namespace ToSic.Eav.WebApi.Sys.Licenses;

public interface ILicenseController: IAppAdminWebApiController
{
    // Replaced by DataSources System.Licenses and System.FeatureStates through query System.SysData.
    //IEnumerable<LicenseDto> Summary();

    /// <summary>
    /// License-upload backend
    /// </summary>
    /// <returns>LicenseFileResultDto</returns>
    /// <exception cref="ArgumentException"></exception>
    // UI test: Apps management > Registration / License > upload a license file.
    LicenseFileResultDto Upload();


    /// <summary>
    /// License-retrieve backend
    /// </summary>
    /// <returns>LicenseFileResultDto</returns>
    /// <exception cref="ArgumentException"></exception>
    // UI test: Apps management > Registration / License > retrieve the registered license.
    LicenseFileResultDto Retrieve();
}
