using System;
using System.Collections.Generic;
using ToSic.Eav.Configuration.Licenses;

namespace ToSic.Eav.Configuration
{
    public partial class FeaturesCatalog
    {
        internal static List<FeatureLicenseRule> ForPatrons = BuildRule(LicenseCatalog.PatronBasic, true);

        public static readonly FeatureDefinition PasteImageFromClipboard = new FeatureDefinition(
            "PasteImageFromClipboard",
            new Guid("f6b8d6da-4744-453b-9543-0de499aa2352"),
            "Paste Image from Clipboard",
            true,
            true,
            "Enable paste image from clipboard into a wysiwyg or file field.",
            FeaturesCatalogRules.Security0Improved,
            ForPatrons
        );

        public static readonly FeatureDefinition NoSponsoredByToSic = new FeatureDefinition(
            "NoSponsoredByToSic",
            new Guid("4f3d0021-1c8b-4286-a33b-3210ed3b2d9a"),
            "No Sponsored-By-2sic messages",
            true,
            true,
            "Hide 'Sponsored by 2sic' messages - for example on the ADAM field.",
            FeaturesCatalogRules.Security0Improved,
            ForPatrons
        );

    }
}
