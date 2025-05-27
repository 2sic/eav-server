using ToSic.Sys.Capabilities.Licenses;

namespace ToSic.Sys.Capabilities.Features;

public class BuiltInLicenseRules
{
    // IMPORTANT
    // The guids of these licenses must match the ones in the 2sxc.org features list
    // So always create the definition there first, then use the GUID of that definition here


    public static List<FeatureLicenseRule> BuildRule(FeatureSet.FeatureSet licDef, bool featureEnabled) =>
        [new(licDef, featureEnabled)];


    public static List<FeatureLicenseRule> ForAllEnabled = BuildRule(BuiltInLicenses.CoreFree, true);
    public static List<FeatureLicenseRule> ForAllDisabled = BuildRule(BuiltInLicenses.CoreFree, false);

    public static List<FeatureLicenseRule> SystemEnabled = BuildRule(BuiltInLicenses.System, true);
    public static List<FeatureLicenseRule> ExtensionEnabled = BuildRule(BuiltInLicenses.Extension, true);

}