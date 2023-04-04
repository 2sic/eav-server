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
            "Edit UI - Enable Translate with Google Translate (WIP/BETA)",
            true,
            true,
            "Allow editors to translate the content using Google Translate. Important: Requires a Google Translate Key and some initial setup.",
            FeaturesCatalogRules.Security0Neutral,
            ForPatronAdvancedCmsAutoEnabled
        );
        

    }
}
