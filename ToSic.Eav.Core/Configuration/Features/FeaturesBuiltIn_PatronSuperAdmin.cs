using System;
using System.Collections.Generic;

namespace ToSic.Eav.Configuration
{
    public partial class BuiltInFeatures
    {
        internal static List<FeatureLicenseRule> ForPatronSuperAdminAutoEnabled = BuildRule(Licenses.BuiltInLicenses.PatronAdvancedCms, true);

        public static readonly FeatureDefinition AppSyncWithSiteFiles = new FeatureDefinition(
            nameof(AppSyncWithSiteFiles),
            new Guid("35694e6b-cd2f-4634-9ecf-5bd6fd14d9a1"),
            "App Sync - Allow Include Site Files",
            false,
            true,
            "Allow site files to be included when synchronizing apps across installations. The files are copied to the App_Data folder for synchronizing.",
            FeaturesCatalogRules.Security0Neutral,
            ForPatronSuperAdminAutoEnabled
        );
        
    }
}
