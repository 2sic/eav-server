using System;
using System.Collections.Generic;

namespace ToSic.Eav.Configuration
{
    public partial class BuiltInFeatures
    {
        internal static List<FeatureLicenseRule> ForPatronSuperAdminAutoEnabled = BuildRule(Licenses.BuiltInLicenses.PatronSuperAdmin, true);

        public static readonly FeatureDefinition AppSyncWithSiteFiles = new FeatureDefinition(
            nameof(AppSyncWithSiteFiles),
            new Guid("35694e6b-cd2f-4634-9ecf-5bd6fd14d9a1"),
            "App Sync - Allow Include Site Files",
            false,
            false,
            "Allow site files to be included when synchronizing apps across installations. The files are copied to the App_Data folder for synchronizing.",
            FeaturesCatalogRules.Security0Neutral,
            ForPatronSuperAdminAutoEnabled
        );
        
        public static readonly FeatureDefinition AppAutoInstallerConfigurable = new FeatureDefinition(
            nameof(AppAutoInstallerConfigurable),
            new Guid("3413786a-de3a-416f-827f-5d4c7cfc11a6"),
            "App Auto-Installer - Make it more configurable.",
            false,
            false,
            "With this system admins can configure what apps can be auto-installed.",
            FeaturesCatalogRules.Security0Neutral,
            ForPatronSuperAdminAutoEnabled
        );

        public static readonly FeatureDefinition DataExportImportBundles = new FeatureDefinition(
            nameof(DataExportImportBundles),
            new Guid("32f4d1e6-764c-4702-9cda-521428aca66c"),
            "Export Data in configured bundles",
            false,
            false,
            "Export Data (Content-Types, Entities) as bundles for repeatable batch export/import.",
            FeaturesCatalogRules.Security0Neutral,
            ForPatronSuperAdminAutoEnabled
        );
    }
}
