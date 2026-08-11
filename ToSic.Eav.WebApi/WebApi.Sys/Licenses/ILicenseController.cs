namespace ToSic.Eav.WebApi.Sys.Licenses;

public interface ILicenseController
{
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