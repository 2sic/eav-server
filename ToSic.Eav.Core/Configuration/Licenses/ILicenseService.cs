using System.Collections.Generic;
using System.Collections.Immutable;

namespace ToSic.Eav.Configuration.Licenses
{
    public interface ILicenseService
    {
        List<LicenseState> All { get; }
        IImmutableDictionary<LicenseDefinition, LicenseState> Enabled { get; }

        bool IsEnabled(LicenseDefinition licenseId);
    }
}