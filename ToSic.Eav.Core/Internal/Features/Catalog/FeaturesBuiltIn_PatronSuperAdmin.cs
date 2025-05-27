using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Capabilities.Licenses;

namespace ToSic.Eav.Internal.Features;

public partial class BuiltInFeatures
{
    internal static List<FeatureLicenseRule> ForPatronSuperAdminAutoEnabled = BuiltInLicenseRules.BuildRule(BuiltInLicenses.PatronSuperAdmin, true);

    public static readonly Feature AppSyncWithSiteFiles = new() {
        NameId = nameof(AppSyncWithSiteFiles),
        Guid = new("35694e6b-cd2f-4634-9ecf-5bd6fd14d9a1"),
        Name = "App Sync - Allow Include Site Files",
        IsPublic = false,
        Ui = false,
        Description = "Allow site files to be included when synchronizing apps across installations. The files are copied to the App_Data folder for synchronizing.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronSuperAdminAutoEnabled
    };

    public static readonly Feature AppAutoInstallerConfigurable = new()
    {
        NameId = nameof(AppAutoInstallerConfigurable),
        Guid = new("3413786a-de3a-416f-827f-5d4c7cfc11a6"),
        Name = "App Auto-Installer - Make it more configurable.",
        IsPublic = false,
        Ui = false,
        Description = "With this system admins can configure what apps can be auto-installed.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronSuperAdminAutoEnabled
    };

    // TODO = NOT PUBLICLY AVAILABLE YET
    public static readonly Feature DataExportImportBundles = new(){
        NameId = nameof(DataExportImportBundles),
        Guid = new("32f4d1e6-764c-4702-9cda-521428aca66c"),
        Name = "Export Data in configured bundles",
        IsPublic = false,
        Ui = false,
        Description = "Export Data (Content-Types, Entities) as bundles for repeatable batch export/import.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronSuperAdminAutoEnabled
    };


    public static readonly Feature AppExportAssetsAdvanced = new(){
        NameId = nameof(AppExportAssetsAdvanced),
        Guid = new("653cb9b6-05a5-4a72-a1c6-141d2b4ae3db"),
        Name = "Full control what asses are exported with an App.",
        IsPublic = false,
        Ui = false,
        Description = "Choose what assets are exported together with an App - so you can exclude certain assets or even skip assets of deleted entities.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronSuperAdminAutoEnabled
    };

}