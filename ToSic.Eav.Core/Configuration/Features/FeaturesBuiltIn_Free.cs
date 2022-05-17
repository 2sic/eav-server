using System;
using System.Collections.Generic;
using ToSic.Eav.Configuration.Licenses;

namespace ToSic.Eav.Configuration
{
    public partial class BuiltInFeatures
    {
        public static List<FeatureLicenseRule> ForAllEnabled = BuildRule(BuiltInLicenses.CoreFree, true);
        public static List<FeatureLicenseRule> ForAllDisabled = BuildRule(BuiltInLicenses.CoreFree, false);

        public const bool ForUi = true;
        public const bool NotForUi = false;
        public const bool Public = true;
        public const bool NotForPublic = false;

        public static readonly FeatureDefinition WysiwygPasteFormatted = new FeatureDefinition(
            "WysiwygPasteFormatted",
            new Guid("1b13e0e6-a346-4454-a1e6-2fb18c047d20"),
            "Paste Formatted Text",
            Public,
            ForUi,
            "Paste formatted text into WYSIWYG TinyMCE",
            new FeatureSecurity(2,
                "Should not affect security, unless a TinyMCE bug allows adding script tags or similar which could result in XSS."),
            ForAllEnabled
        );




        public static readonly FeatureDefinition EditUiShowNotes = new FeatureDefinition(
            "EditUiShowNotes",
            new Guid("945320af-9ba9-4117-87cb-d63815e99fd4"),
            "Edit UI: Show notes button",
            Public,
            ForUi,
            "",
            FeaturesCatalogRules.Security0Neutral,
            ForAllEnabled
        );

        public static readonly FeatureDefinition EditUiShowMetadataFor = new FeatureDefinition(
            "EditUiShowMetadataFor",
            new Guid("717b5d0a-07b1-41ec-a670-ec9665cd4af1"),
            "Edit UI: Show information if something is Metadata-For",
            Public,
            ForUi,
            "",
            FeaturesCatalogRules.Security0Neutral,
            ForAllEnabled
        );


    }
}
