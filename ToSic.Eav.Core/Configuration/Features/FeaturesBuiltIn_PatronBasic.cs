using System;
using System.Collections.Generic;

namespace ToSic.Eav.Configuration
{
    public partial class BuiltInFeatures
    {
        public static List<FeatureLicenseRule> ForPatronBasicEnabled = BuildRule(Licenses.BuiltInLicenses.PatronBasic, true);

        public static readonly FeatureDefinition PasteImageFromClipboard = new FeatureDefinition(
            "PasteImageFromClipboard",
            new Guid("f6b8d6da-4744-453b-9543-0de499aa2352"),
            "Paste Image from Clipboard",
            true,
            true,
            "Enable paste image from clipboard into a wysiwyg or file field.",
            FeaturesCatalogRules.Security0Neutral,
            ForPatronBasicEnabled
        );

        public static readonly FeatureDefinition NoSponsoredByToSic = new FeatureDefinition(
            "NoSponsoredByToSic",
            new Guid("4f3d0021-1c8b-4286-a33b-3210ed3b2d9a"),
            "No Sponsored-By-2sic messages",
            true,
            true,
            "Hide 'Sponsored by 2sic' messages - for example on the ADAM field.",
            FeaturesCatalogRules.Security0Neutral,
            ForPatronBasicEnabled
        );

        public static readonly FeatureDefinition EditUiGpsCustomDefaults = new FeatureDefinition(
            "EditUiGpsCustomDefaults",
            new Guid("19736d09-7424-43fc-9a65-04b53bf7f95c"),
            "Set custom defaults for the GPS Picker.",
            false,
            false,
            "By default the GPS-Picker will start in Switzerland. If you enable this, you can reconfigure it in the settings.",
            FeaturesCatalogRules.Security0Neutral,
            ForPatronBasicEnabled
        );

    }
}
