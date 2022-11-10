using System;

namespace ToSic.Eav.Configuration.Licenses
{
    public class LicenseDisabledException: Exception
    {
        public LicenseDisabledException(LicenseDefinition lic): base($"Error: Requires License {lic.NameId}")
        {
        }
    }
}
