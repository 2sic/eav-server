using System;
using System.Collections.Generic;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Configuration
{
    [PrivateApi]
    public class FeaturesCatalog
    {
        internal static FeatureSecurity Security0 = new FeatureSecurity(0, "Actually increases security.");

        internal static List<FeatureLicenseRule> ForPatreons = new List<FeatureLicenseRule>
        {
            new FeatureLicenseRule(LicenseCatalog.PatreonSupporter, true)
        };
        internal static List<FeatureLicenseRule> ForAll = new List<FeatureLicenseRule>
        {
            new FeatureLicenseRule(LicenseCatalog.CoreFree, false)
        };
        internal static List<FeatureLicenseRule> ForBeta = new List<FeatureLicenseRule>
        {
            new FeatureLicenseRule(LicenseCatalog.CoreBeta, false)
        };

        public static readonly FeatureDefinition PasteImageFromClipboard = new FeatureDefinition(
            "PasteImageFromClipboard",
            new Guid("f6b8d6da-4744-453b-9543-0de499aa2352"),
            "Paste from Clipboard",
            true,
            true,
            "Enable paste image from clipboard into a wysiwyg or file field.",
            Security0,
            ForPatreons
        );

        public static readonly FeatureDefinition WysiwygPasteFormatted = new FeatureDefinition(
            "WysiwygPasteFormatted",
            new Guid("1b13e0e6-a346-4454-a1e6-2fb18c047d20"),
            "Paste Formatted Text",
            true,
            true,
            "Paste formatted text into WYSIWYG TinyMCE",
            new FeatureSecurity(2,
                "Should not affect security, unless a TinyMCE bug allows adding script tags or similar which could result in XSS."),
            ForAll
        );

        public static readonly FeatureDefinition PublicEditForm = new FeatureDefinition(
            "PublicEditForm",
            new Guid("d93baf71-74c6-4956-9fe0-8281acdfd14a"),
            "Public use of Edit Form",
            true,
            false,
            "Allows non-editors to use the Edit-Form to edit/create data. <br>" +
            "This is for commands like @Edit.Enable(...) or certain IPageService.Activate(...)",
            new FeatureSecurity(4, "Could affect security if a bug would allow saving data without permission checks."),
            ForAll
        );


        public static readonly FeatureDefinition PublicUploadFiles = new FeatureDefinition(
            "PublicUploadFiles",
            new Guid("79b9f5f8-d104-458b-8e8f-9f4a11c5935e"),
            "Public Upload of Files",
            true,
            false,
            "Allow non-editors to upload files. <br>" +
            "With this enabled, public forms will allow file-upload with ADAM. Otherwise it will be disabled. <br>" +
            "Note that each of these fields must also have the permissions set allowing an upload. This is to prevent uploads on fields which were just meant to hold a link.",
            new FeatureSecurity(4, "Could affect security if a bug allows saving files with bad extensions."),
            ForAll
        );

        public static readonly FeatureDefinition SaveInAdamApi = new FeatureDefinition(
            "SaveInAdamApi",
            new Guid("ecdab0f6-4692-4544-b1e7-72581f489f6a"),
            "SaveInAdam API",
            false,
            false,
            "Enable the SaveInAdam command in Razor / C#. Otherwise, SaveInAdam(...) will be refused, and custom WebAPIs trying to do this (like Mobius forms with Upload) will throw an error.",
            new FeatureSecurity(4, "Could affect security if a bug allows saving files with bad extensions."),
            ForAll
        );

        public static readonly FeatureDefinition PermissionCheckUsers = new FeatureDefinition(
            "PermissionCheckUsers",
            new Guid("47c71ee9-ac7b-45bf-a08b-dfc8ce7c7775"),
            "Permission Check based on User",
            true,
            false,
            "Enable permission checks by user (User ID)",
            new FeatureSecurity(1, "Could affect security if a bug in security checks doesn't correctly match IDs."),
            ForAll
        );

        public static readonly FeatureDefinition PermissionCheckGroups = new FeatureDefinition(
            "PermissionCheckGroups",
            new Guid("0fd479cc-300f-47fd-88fd-8f2fe092bc09"),
            "Permission Check based on Group",
            true,
            false,
            "Enable permission checks to use the Groups a user belongs to based on the Group/Role-ID",
            new FeatureSecurity(1, "Could affect security if a bug in security checks doesn't correctly match IDs."),
            ForAll
        );

        // TODO: Probably change how this setting is used, so it's "Enable..." and defaults to true ?
        // ATM only used in azing, so still easy to change

        public static readonly FeatureDefinition BlockFileResolveOutsideOfEntityAdam = new FeatureDefinition(
            "BlockFileResolveOutsideOfEntityAdam-BETA",
            new Guid("702f694c-53bd-4d03-b75c-4dad9c4fb852"),
            "BlockFileResolveOutsideOfEntityAdam (BETA, not final)",
            false,
            false,
            "If enabled, then links like 'file:72' will only work if the file is inside the ADAM of the current Entity.",
            Security0,
            ForBeta
        );


        public static readonly FeatureDefinition WebFarmCache = new FeatureDefinition(
            "WebFarmCache",
            new Guid("11c0fedf-16a7-4596-900c-59e860b47965"),
            "Web Farm Cache",
            false,
            false,
            "Enables WebFarm Cache use in Dnn",
            Security0,
            new List<FeatureLicenseRule> { new FeatureLicenseRule(LicenseCatalog.WebFarm, false) }
        );

        // WIP / Beta in v13
        public static readonly FeatureDefinition ImageServiceMultiFormat = new FeatureDefinition(
            "ImageServiceMultiFormat",
            new Guid("4262df94-3877-4a5a-ac86-20b4f9b38e87"),
            "Image Service Activates Multiple Formats",
            false,
            false,
            "Enables the ImageService to also provide WebP as better alternatives to Jpg and Png",
            Security0,
            ForPatreons
        );

        // WIP / Beta in v13
        // todo: mention decorator in the settings?
        public static readonly FeatureDefinition GlobalAppsWIP = new FeatureDefinition(
            "GlobalAppsEnabled-WIP",
            new Guid("bb6656ef-fb81-4943-bf88-297e516d2616"),
            "Apps can be shared globally",
            false,
            false,
            "todo",
            Security0,
            new List<FeatureLicenseRule>() { new FeatureLicenseRule(LicenseCatalog.SitesFarm, true) }
        );

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
        public static List<FeatureDefinition> Initial = new List<FeatureDefinition>
        {
            // Released features since the dawn of features
            PublicEditForm,
            PublicUploadFiles,
            SaveInAdamApi,
            PermissionCheckUsers,
            PermissionCheckGroups,

            // Beta features - meant for Patreons
            PasteImageFromClipboard,
            WysiwygPasteFormatted,

            // 2sxc 10.24+
            WebFarmCache,

            // 2sxc 13 - for Patreons
            ImageServiceMultiFormat,

            // 2sxc 13 - Global Apps
            GlobalAppsWIP,
        };
    }
}
