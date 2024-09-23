using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features;

public partial class BuiltInFeatures
{
    internal static List<FeatureLicenseRule> ForPatronSuperAdminAutoEnabled = BuildRule(BuiltInLicenses.PatronSuperAdmin, true);

    public static readonly Feature AppSyncWithSiteFiles = new(
        nameof(AppSyncWithSiteFiles),
        new("35694e6b-cd2f-4634-9ecf-5bd6fd14d9a1"),
        "App Sync - Allow Include Site Files",
        false,
        false,
        "Allow site files to be included when synchronizing apps across installations. The files are copied to the App_Data folder for synchronizing.",
        FeaturesCatalogRules.Security0Neutral,
        ForPatronSuperAdminAutoEnabled
    );
        
    public static readonly Feature AppAutoInstallerConfigurable = new(
        nameof(AppAutoInstallerConfigurable),
        new("3413786a-de3a-416f-827f-5d4c7cfc11a6"),
        "App Auto-Installer - Make it more configurable.",
        false,
        false,
        "With this system admins can configure what apps can be auto-installed.",
        FeaturesCatalogRules.Security0Neutral,
        ForPatronSuperAdminAutoEnabled
    );

    // TODO: NOT PUBLICLY AVAILABLE YET
    public static readonly Feature DataExportImportBundles = new(
        nameof(DataExportImportBundles),
        new("32f4d1e6-764c-4702-9cda-521428aca66c"),
        "Export Data in configured bundles",
        false,
        false,
        "Export Data (Content-Types, Entities) as bundles for repeatable batch export/import.",
        FeaturesCatalogRules.Security0Neutral,
        ForPatronSuperAdminAutoEnabled
    );


    public static readonly Feature AppExportAssetsAdvanced = new(
        nameof(AppExportAssetsAdvanced),
        new("653cb9b6-05a5-4a72-a1c6-141d2b4ae3db"),
        "Full control what asses are exported with an App.",
        false,
        false,
        "Choose what assets are exported together with an App - so you can exclude certain assets or even skip assets of deleted entities.",
        FeaturesCatalogRules.Security0Neutral,
        ForPatronSuperAdminAutoEnabled
    );

}