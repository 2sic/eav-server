using System;

namespace ToSic.Eav.Configuration.Licenses
{
    public class LicenseException: Exception
    {
        public LicenseException(LicenseDefinition lic): base($"Error: Requires License {lic.NameId}")
        {
        }
    }
}
