using System;
using System.Collections.Generic;

namespace ToSic.Eav.Configuration
{
    public partial class BuiltInFeatures
    {
        internal static List<FeatureLicenseRule> ForPatronAdvancedCmsAutoEnabled = BuildRule(Licenses.BuiltInLicenses.PatronAdvancedCms, true);


        public static readonly FeatureDefinition EditUiTranslateWithGoogle = new FeatureDefinition(
            nameof(EditUiTranslateWithGoogle),
            new Guid("353186b4-7e19-41fb-9dca-c408c26e43d7"),
            "Edit UI - Enable Translate with Google Translate",
            true,
            true,
            "Allow editors to translate the content using Google Translate. Important: Requires a Google Translate Key and some initial setup.",
            FeaturesCatalogRules.Security0Neutral,
            ForPatronAdvancedCmsAutoEnabled
        );
        
        public static readonly FeatureDefinition LanguagesAdvancedFallback = new FeatureDefinition(
            nameof(LanguagesAdvancedFallback),
            new Guid("95bb2232-ec19-4f9c-adf9-9df07d841cc8"),
            "Languages - Customize language fallback sequence",
            true,
            false,
            "Allow admins so specify language fallback at a granular level.",
            FeaturesCatalogRules.Security0Neutral,
            ForPatronAdvancedCmsAutoEnabled
        );

        public static readonly FeatureDefinition CopyrightManagement = new FeatureDefinition(
            nameof(CopyrightManagement),
            new Guid("2114297a-d1e7-40d2-88d7-e44cd1111bfa"),
            "Copyright Management for Content (WIP/Beta v17)",
            false,
            true,
            "If enabled, Copyright Management will appear in image toolbars and in future do more. ",
            FeaturesCatalogRules.Security0Neutral,
            ForPatronAdvancedCmsAutoEnabled
        );

    }
}
