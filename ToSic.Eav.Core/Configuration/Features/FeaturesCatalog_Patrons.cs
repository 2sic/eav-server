using System;
using System.Collections.Generic;
using ToSic.Eav.Configuration.Licenses;

namespace ToSic.Eav.Configuration
{
    public partial class FeaturesCatalog
    {
        internal static List<FeatureLicenseRule> ForPatrons = BuildRule(LicenseCatalog.Patron);

        public static readonly FeatureDefinition PasteImageFromClipboard = new FeatureDefinition(
            "PasteImageFromClipboard",
            new Guid("f6b8d6da-4744-453b-9543-0de499aa2352"),
            "Paste Image from Clipboard",
            true,
            true,
            "Enable paste image from clipboard into a wysiwyg or file field.",
            FeaturesCatalogRules.Security0,
            ForPatrons
        );

        public static readonly FeatureDefinition NoSponsoredByToSic = new FeatureDefinition(
            "NoSponsoredByToSic",
            new Guid("4f3d0021-1c8b-4286-a33b-3210ed3b2d9a"),
            "No Sponsored-By-2sic messages",
            true,
            true,
            "Hide 'Sponsored by 2sic' messages - for example on the ADAM field.",
            FeaturesCatalogRules.Security0,
            ForPatrons
        );

        // WIP / Beta in v13
        public static readonly FeatureDefinition ImageServiceMultiFormat = new FeatureDefinition(
            "ImageServiceMultiFormat",
            new Guid("4262df94-3877-4a5a-ac86-20b4f9b38e87"),
            "Image Service Activates Multiple Formats",
            false,
            false,
            "Enables the ImageService to also provide WebP as better alternatives to Jpg and Png", 
            FeaturesCatalogRules.Security0,
            ForPatrons
        );

    }
}
