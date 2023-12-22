using System;
using System.Collections.Generic;

namespace ToSic.Eav.WebApi.Sys.Licenses;

public interface ILicenseController
{
    /// <summary>
    /// Gives an array of License (sort by priority)
    /// </summary>
    /// <returns></returns>
    IEnumerable<LicenseDto> Summary();

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