using ToSic.Eav.Documentation;
using ToSic.Lib.Logging;
using static ToSic.Eav.Configuration.BuiltInFeatures;

namespace ToSic.Eav.Configuration
{
    [PrivateApi]
    public class FeaturesCatalog: GlobalCatalogBase<FeatureDefinition>
    {
        public FeaturesCatalog(History logHistory): base(logHistory, LogNames.Eav + ".FeatCt", new CodeRef())
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
                EditUiTranslateWithGoogle, // WIP/Beta, v15

                // Patron SuperAdmin
                AppSyncWithSiteFiles,  // WIP/Beta

                // Patron Infrastructure
                SqlCompressDataTimeline, // WIP / Beta v15

                // 2sxc 10.24+
                WebFarmCache,
                WebFarmCacheDebug,

                // 2sxc 13 - Enterprise CMS
                SharedApps,
                PermissionsByLanguage,
                EditUiDisableDraft,

                // Beta features
                BlockFileResolveOutsideOfEntityAdam
            );
        }

        // TODO: MAYBE SUB-FEATURES FOR global apps
        // - Inherit views - auto-on if global on
        // - Inherit data - auto-on if global on
        // - Inherit queries
        
    }
}
