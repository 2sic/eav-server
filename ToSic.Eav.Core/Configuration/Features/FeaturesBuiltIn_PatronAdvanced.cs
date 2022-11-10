using System;
using System.Collections.Generic;

namespace ToSic.Eav.Configuration
{
    public partial class BuiltInFeatures
    {
        internal static List<FeatureLicenseRule> ForPatronAdvanced = BuildRule(Licenses.BuiltInLicenses.PatronAdvanced, true);

        public static readonly FeatureDefinition AppSyncWithSiteFiles = new FeatureDefinition(
            "AppSyncWithSiteFiles",
            new Guid("35694e6b-cd2f-4634-9ecf-5bd6fd14d9a1"),
            "App Sync - Allow Include Site Files",
            false,
            true,
            "Allow site files to be included when synchronizing apps across installations. The files are copied to the App_Data folder for synchronizing.",
            FeaturesCatalogRules.Security0Neutral,
            ForPatronAdvanced
        );

        public static readonly FeatureDefinition EditUiTranslateWithGoogle = new FeatureDefinition(
            "EditUiTranslateWithGoogle",
            new Guid("353186b4-7e19-41fb-9dca-c408c26e43d7"),
            "Edit UI - Enable Translate with Google Translate (WIP/BETA)",
            false,
            true,
            "Allow editors to translate the content using Google Translate. Important: Requires a Google Translate Key and some initial setup.",
            FeaturesCatalogRules.Security0Neutral,
            ForPatronAdvanced
        );
    }
}
