namespace ToSic.Eav.WebApi.Sys.Licenses;

public interface ILicenseController
{
    // Replaced by DataSources System.Licenses and System.FeatureStates through query System.SysData.
    //IEnumerable<LicenseDto> Summary();

    /// <summary>
    /// License-upload backend
    /// </summary>
    /// <returns>LicenseFileResultDto</returns>
    /// <exception cref="ArgumentException"></exception>
    LicenseFileResultDto Upload();


    /// <summary>
    /// License-retrieve backend
    /// </summary>
    /// <returns>LicenseFileResultDto</returns>
    /// <exception cref="ArgumentException"></exception>
    LicenseFileResultDto Retrieve();
}