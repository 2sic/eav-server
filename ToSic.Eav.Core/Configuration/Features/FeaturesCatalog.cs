using System.Collections.Generic;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Configuration
{
    [PrivateApi]
    public partial class FeaturesCatalog
    {
        // IMPORTANT
        // The guids of these licenses must match the ones in the 2sxc.org features list
        // So always create the definition there first, then use the GUID of that definition here

        // Todo: Lightspeed Cache


        // TODO: MAYBE SUB-FEATURES FOR global apps
        // - Inherit views - auto-on if global on
        // - Inherit data - auto-on if global on
        // - Inherit queries

        /// <summary>
        /// The catalog contains known features, and knows if they are used in the UI
        /// This is important, because the installation specific list often won't know about
        /// Ui or not. 
        /// </summary>
        /// <remarks>
        /// this is a temporary solution, because most features are from 2sxc (not eav)
        /// so later on this must be injected or something
        /// </remarks>
        [PrivateApi]
        public static List<FeatureDefinition> Initial => _initial ?? (_initial = BuildFeatureDefinitions());
        private static List<FeatureDefinition> _initial;

        private static List<FeatureDefinition> BuildFeatureDefinitions() =>
            new List<FeatureDefinition>
            {
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
                ImageServiceMultiFormat,    // v13
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
                BlockFileResolveOutsideOfEntityAdam,
                RazorThrowPartial,
                RenderThrowPartialSystemAdmin,
            };


        internal static List<FeatureLicenseRule> BuildRule(LicenseDefinition licDef, bool enabled) => new List<FeatureLicenseRule>
        {
            new FeatureLicenseRule(licDef, enabled)
        };
    }
}
