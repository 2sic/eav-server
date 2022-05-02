using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
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

                //// Patrons Perfectionist
                //ImageServiceMultiFormat, // v13
                //ImageServiceMultipleSizes,
                //ImageServiceSetSizes,
                //ImageServiceUseFactors,

                // 2sxc 10.24+
                WebFarmCache,

                // 2sxc 13 - Global Apps
                SharedApps,
                PermissionsByLanguage,

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
