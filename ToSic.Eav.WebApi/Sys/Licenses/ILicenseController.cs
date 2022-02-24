using System.Collections.Generic;

namespace ToSic.Eav.WebApi.Sys.Licenses
{
    public interface ILicenseController
    {
        IEnumerable<LicenseDto> Summary();
    }
}