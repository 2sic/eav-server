using ToSic.Eav.Configuration.Licenses;

namespace ToSic.Eav.Configuration
{
    internal class FeatureLicenseRule
    {
        public FeatureLicenseRule(LicenseType licenseType, bool enabled)
        {
            LicenseType = licenseType;
            DefaultEnabled = enabled;
        }

        public LicenseType LicenseType { get; }

        public bool DefaultEnabled { get; } = true;
    }
}
