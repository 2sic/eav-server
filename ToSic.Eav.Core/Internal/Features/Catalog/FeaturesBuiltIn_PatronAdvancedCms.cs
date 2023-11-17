using System;
using System.Collections.Generic;
using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features
{
    public partial class BuiltInFeatures
    {
        internal static List<FeatureLicenseRule> ForPatronAdvancedCmsAutoEnabled = BuildRule(BuiltInLicenses.PatronAdvancedCms, true);


        public static readonly Feature EditUiTranslateWithGoogle = new Feature(
            nameof(EditUiTranslateWithGoogle),
            new Guid("353186b4-7e19-41fb-9dca-c408c26e43d7"),
            "Edit UI - Enable Translate with Google Translate",
            true,
            true,
            "Allow editors to translate the content using Google Translate. Important: Requires a Google Translate Key and some initial setup.",
            FeaturesCatalogRules.Security0Neutral,
            ForPatronAdvancedCmsAutoEnabled
        );
        
        public static readonly Feature LanguagesAdvancedFallback = new Feature(
            nameof(LanguagesAdvancedFallback),
            new Guid("95bb2232-ec19-4f9c-adf9-9df07d841cc8"),
            "Languages - Customize language fallback sequence",
            true,
            false,
            "Allow admins so specify language fallback at a granular level.",
            FeaturesCatalogRules.Security0Neutral,
            ForPatronAdvancedCmsAutoEnabled
        );

        public static readonly Feature FieldShareConfigManagement = new Feature(
            nameof(FieldShareConfigManagement),
            new Guid("e0398b1f-32ca-4734-b49a-83ff894e352e"),
            "Field Sharing - Enable Configure in Admin UI",
            false,
            true,
            "Enable Field Sharing Management directly in the UI.",
            FeaturesCatalogRules.Security0Neutral,
            ForPatronAdvancedCmsAutoEnabled
        );


    }
}
