﻿using ToSic.Eav.Internal.Catalogs;
using ToSic.Eav.SysData;
using static ToSic.Eav.Internal.Features.BuiltInFeatures;

namespace ToSic.Eav.Internal.Features;

[PrivateApi]
public class FeaturesCatalog: GlobalCatalogBase<Feature>
{
    public FeaturesCatalog(ILogStore logStore): base(logStore, $"{EavLogs.Eav}.FeatCt", new())
    {
        Register(
            // Released features since the dawn of features
            PublicEditForm,
            PublicUploadFiles,
            SaveInAdamApi,
            PermissionCheckUsers,
            PermissionCheckGroups,

            // Free Edit UI features for all
            EditUiShowNotes,
            EditUiShowMetadataFor,
            WysiwygPasteFormatted,
            EditUiAllowDebugModeForEditors,

            // Features for Patrons Basic
            PasteImageFromClipboard,
            NoSponsoredByToSic,
            EditUiGpsCustomDefaults,    // new v15

            // Features for Patrons Advanced CMS
            EditUiTranslateWithGoogle,  // v15
            LanguagesAdvancedFallback,  // v16.04
            CopyrightManagement,        // v16.08 / v17
            ContentTypeFieldsReuseDefinitions, // v16.08

            // Patron Advanced CMS - Picker - v18.01+
            PickerUiCheckbox,
            PickerUiRadio,
            PickerSourceCsv,
            PickerSourceAppAssets,
            PickerFormulas,
            PickerUiMoreInfo,

            // Patron SuperAdmin
            AppSyncWithSiteFiles,           // v15
            AppAutoInstallerConfigurable,   // v15
            DataExportImportBundles,        // v15.01
            AppExportAssetsAdvanced,        // v18

            // Patron Infrastructure
            SqlCompressDataTimeline, // v15
            SqlLoadPerformance,      // v18

            // 2sxc 10.24+
            WebFarmCache,
            WebFarmCacheDebug,

            // 2sxc 13 - Enterprise CMS
            SharedApps,
            PermissionsByLanguage,
            EditUiDisableDraft,

            // Beta features
            AdamRestrictLookupToEntity

#if DEBUG
            // Testing features to verify features
            ,
            TestingFeature001
#endif
        );
    }

    // TODO: MAYBE SUB-FEATURES FOR global apps
    // - Inherit views - auto-on if global on
    // - Inherit data - auto-on if global on
    // - Inherit queries
        
}