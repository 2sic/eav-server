using ToSic.Eav.Configuration.Licenses;

namespace ToSic.Eav.Configuration
{
    public class FeatureLicenseRule
    {
        public FeatureLicenseRule(LicenseDefinition licenseDefinition, bool enabled)
        {
            LicenseDefinition = licenseDefinition;
            EnableFeatureByDefault = enabled;
        }

        public LicenseDefinition LicenseDefinition { get; }

        public bool EnableFeatureByDefault { get; } = true;
    }
}
