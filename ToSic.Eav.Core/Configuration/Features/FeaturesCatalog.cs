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
                //RazorThrowPartial,
                //RenderThrowPartialSystemAdmin
            );
        }

        protected override string GetKey(FeatureDefinition item) => item.NameId;


        // TODO: MAYBE SUB-FEATURES FOR global apps
        // - Inherit views - auto-on if global on
        // - Inherit data - auto-on if global on
        // - Inherit queries

        ///// <summary>
        ///// The catalog contains known features, and knows if they are used in the UI
        ///// This is important, because the installation specific list often won't know about
        ///// Ui or not. 
        ///// </summary>
        ///// <remarks>
        ///// this is a temporary solution, because most features are from 2sxc (not eav)
        ///// so later on this must be injected or something
        ///// </remarks>
        //[PrivateApi]
        //public static List<FeatureDefinition> Initial => _initial ?? (_initial = BuildFeatureDefinitions());
        //private static List<FeatureDefinition> _initial;

        //private static List<FeatureDefinition> BuildFeatureDefinitions() =>
        //    new List<FeatureDefinition>
        //    {
        //        // Released features since the dawn of features
        //        PublicEditForm,
        //        PublicUploadFiles,
        //        SaveInAdamApi,
        //        PermissionCheckUsers,
        //        PermissionCheckGroups,

        //        // Free Edit UI features for all
        //        EditUiShowNotes,
        //        EditUiShowMetadataFor,
        //        EditUiAllowDebugModeForEditors,

        //        // Features for Patreons
        //        PasteImageFromClipboard,
        //        WysiwygPasteFormatted,
        //        NoSponsoredByToSic,

        //        // Patrons Perfectionist
        //        ImageServiceMultiFormat,    // v13
        //        ImageServiceMultipleSizes,
        //        ImageServiceSetSizes,
        //        ImageServiceUseFactors,
        //        LightSpeedOutputCache,

        //        // 2sxc 10.24+
        //        WebFarmCache,

        //        // 2sxc 13 - Global Apps
        //        SharedApps,
        //        PermissionsByLanguage,

        //        // Beta features
        //        BlockFileResolveOutsideOfEntityAdam,
        //        RazorThrowPartial,
        //        RenderThrowPartialSystemAdmin,
        //    };


    }
}
