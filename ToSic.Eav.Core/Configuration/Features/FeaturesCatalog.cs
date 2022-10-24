using ToSic.Lib.Logging;
using ToSic.Lib.Documentation;
using static ToSic.Eav.Configuration.BuiltInFeatures;

namespace ToSic.Eav.Configuration
{
    [PrivateApi]
    public class FeaturesCatalog: GlobalCatalogBase<FeatureDefinition>
    {
        public FeaturesCatalog(LogHistory logHistory): base(logHistory, LogNames.Eav + ".FeatCt", new CodeRef())
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
                EditUiAllowDebugModeForEditors,

                // Features for Patreons
                PasteImageFromClipboard,
                WysiwygPasteFormatted,
                NoSponsoredByToSic,

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
