using System;
using System.Collections.Generic;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Configuration
{
    [PrivateApi]
    public class FeaturesCatalog
    {
        public static readonly Feature PasteImageFromClipboard = new Feature(
            "PasteImageFromClipboard",
            new Guid("f6b8d6da-4744-453b-9543-0de499aa2352"),
            "Paste from Clipboard",
            true,
            true,
            "Enable paste image from clipboard into a wysiwyg or file field.");

        public static readonly Feature WysiwygPasteFormatted = new Feature(
            "WysiwygPasteFormatted",
            new Guid("1b13e0e6-a346-4454-a1e6-2fb18c047d20"),
            "Paste Formatted Text",
            true,
            true,
            "Paste formatted text into WYSIWYG TinyMCE");

        public static readonly Feature PublicEditForm = new Feature(
            "PublicEditForm",
            new Guid("d93baf71-74c6-4956-9fe0-8281acdfd14a"),
            "Public use of Edit Form",
            true,
            false,
            "Allows non-editors to use the Edit-Form to edit/create data. <br>" +
            "This is for commands like @Edit.Enable(...) or certain IPageService.Activate(...)");


        public static readonly Feature PublicUploadFiles = new Feature(
            "PublicUploadFiles",
            new Guid("79b9f5f8-d104-458b-8e8f-9f4a11c5935e"),
            "Public Upload of Files",
            true,
            false,
            "Allow non-editors to upload files. <br>" +
            "With this enabled, public forms will allow file-upload with ADAM. Otherwise it will be disabled. <br>" +
            "Note that each of these fields must also have the permissions set allowing an upload. This is to prevent uploads on fields which were just meant to hold a link. ");

        public static readonly Feature SaveInAdamApi = new Feature(
            "SaveInAdamApi",
            new Guid("ecdab0f6-4692-4544-b1e7-72581f489f6a"),
            "SaveInAdam API",
            false,
            false,
            "Enable the SaveInAdam command in Razor / C#. Otherwise, SaveInAdam(...) will be refused, and custom WebAPIs trying to do this (like Mobius forms with Upload) will throw an error."
        );

        public static readonly Feature PermissionCheckUsers = new Feature(
            "PermissionCheckUsers",
            new Guid("47c71ee9-ac7b-45bf-a08b-dfc8ce7c7775"),
            "Permission Check based on User",
            true,
            false,
            "Enable permission checks by user (User ID)");

        public static readonly Feature PermissionCheckGroups = new Feature(
            "PermissionCheckGroups",
            new Guid("0fd479cc-300f-47fd-88fd-8f2fe092bc09"),
            "Permission Check based on Group",
            true,
            false,
            "Enable permission checks to use the Groups a user belongs to based on the Group/Role-ID");

        // TODO: Probably change how this setting is used, so it's "Enable..." and defaults to true ?
        // ATM only used in azing, so still easy to change

        public static readonly Feature BlockFileResolveOutsideOfEntityAdam = new Feature(
            "BlockFileResolveOutsideOfEntityAdam-BETA",
            new Guid("702f694c-53bd-4d03-b75c-4dad9c4fb852"),
            "BlockFileResolveOutsideOfEntityAdam (BETA, not final)",
            false,
            false,
            "If enabled, then links like 'file:72' will only work if the file is inside the ADAM of the current Entity.");


        public static readonly Feature WebFarmCache = new Feature(
            "WebFarmCache",
            new Guid("11c0fedf-16a7-4596-900c-59e860b47965"),
            "Web Farm Cache",
            false,
            false,
            "Enables WebFarm Cache use in Dnn");

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
        public static FeatureList Initial = new FeatureList(new List<Feature>
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
        });
    }
}
