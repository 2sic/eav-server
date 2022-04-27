using ToSic.Eav.Documentation;
using static ToSic.Eav.Configuration.FeaturesBuiltIn;

namespace ToSic.Eav.Configuration
{
    [PrivateApi]
    public class FeaturesCatalog: GlobalCatalogBase<FeatureDefinition>
    {
        public FeaturesCatalog()
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

                // Patrons Perfectionist
                ImageServiceMultiFormat, // v13
                ImageServiceMultipleSizes,
                ImageServiceSetSizes,
                ImageServiceUseFactors,
                LightSpeedOutputCache,

                // 2sxc 10.24+
                WebFarmCache,

                // 2sxc 13 - Global Apps
                SharedApps,
                PermissionsByLanguage,

                // Beta features
                BlockFileResolveOutsideOfEntityAdam
            );
        }

        protected override string GetKey(FeatureDefinition item) => item.NameId;


        // TODO: MAYBE SUB-FEATURES FOR global apps
        // - Inherit views - auto-on if global on
        // - Inherit data - auto-on if global on
        // - Inherit queries
        
    }
}
