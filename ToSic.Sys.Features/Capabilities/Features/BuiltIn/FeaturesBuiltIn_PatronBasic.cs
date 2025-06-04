using ToSic.Sys.Capabilities.Licenses;

namespace ToSic.Sys.Capabilities.Features;

public partial class BuiltInFeatures
{
    public static List<FeatureLicenseRule> ForPatronBasicEnabled = BuiltInLicenseRules.BuildRule(BuiltInLicenses.PatronBasic, true);

    public static readonly Feature PasteImageFromClipboard = new()
    {
        NameId = "PasteImageFromClipboard",
        Guid = new("f6b8d6da-4744-453b-9543-0de499aa2352"),
        Name = "Paste Image from Clipboard",
        IsPublic = true,
        Ui = true,
        Description = "Enable paste image from clipboard into a wysiwyg or file field.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronBasicEnabled
    };

    public static readonly Feature NoSponsoredByToSic = new()
    {
        NameId = "NoSponsoredByToSic",
        Guid = new("4f3d0021-1c8b-4286-a33b-3210ed3b2d9a"),
        Name = "No Sponsored-By-2sic messages",
        IsPublic = true,
        Ui = true,
        Description = "Hide 'Sponsored by 2sic' messages - for example on the ADAM field.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronBasicEnabled
    };

    public static readonly Feature EditUiGpsCustomDefaults = new()
    {
        NameId = "EditUiGpsCustomDefaults",
        Guid = new("19736d09-7424-43fc-9a65-04b53bf7f95c"),
        Name = "Set custom defaults for the GPS Picker.",
        IsPublic = false,
        Ui = false,
        Description = "By default the GPS-Picker will start in Switzerland. If you enable this, you can reconfigure it in the settings.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronBasicEnabled
    };

}