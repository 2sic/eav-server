using System;
using System.Collections.Generic;
using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features
{
    public partial class BuiltInFeatures
    {
        public static List<FeatureLicenseRule> ForAllEnabled = BuildRule(BuiltInLicenses.CoreFree, true);
        public static List<FeatureLicenseRule> ForAllDisabled = BuildRule(BuiltInLicenses.CoreFree, false);

        public static List<FeatureLicenseRule> SystemEnabled = BuildRule(BuiltInLicenses.System, true);
        public static List<FeatureLicenseRule> ExtensionEnabled = BuildRule(BuiltInLicenses.Extension, true);

        public const bool ForUi = true;
        public const bool NotForUi = false;
        public const bool Public = true;
        public const bool NotForPublic = false;

        public static readonly Feature WysiwygPasteFormatted = new(
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




        public static readonly Feature EditUiShowNotes = new(
            "EditUiShowNotes",
            new Guid("945320af-9ba9-4117-87cb-d63815e99fd4"),
            "Edit UI: Show notes button",
            Public,
            ForUi,
            "",
            FeaturesCatalogRules.Security0Neutral,
            ForAllEnabled
        );

        public static readonly Feature EditUiShowMetadataFor = new(
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
