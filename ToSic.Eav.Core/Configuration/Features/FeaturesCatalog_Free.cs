using System;
using System.Collections.Generic;
using ToSic.Eav.Configuration.Licenses;

namespace ToSic.Eav.Configuration
{
    public partial class FeaturesCatalog
    {
        internal static List<FeatureLicenseRule> ForAllEnabled = BuildRule(LicenseCatalog.CoreFree, true);
        internal static List<FeatureLicenseRule> ForAllDisabled = BuildRule(LicenseCatalog.CoreFree, false);

        public const bool ForUi = true;
        public const bool NotForUi = false;
        public const bool Public = true;
        public const bool NotForPublic = false;

        public static readonly FeatureDefinition WysiwygPasteFormatted = new FeatureDefinition(
            "WysiwygPasteFormatted",
            new Guid("1b13e0e6-a346-4454-a1e6-2fb18c047d20"),
            "Paste Formatted Text",
            Public,
            ForUi,
            "Paste formatted text into WYSIWYG TinyMCE",
            new FeatureSecurity(2,
                "Should not affect security, unless a TinyMCE bug allows adding script tags or similar which could result in XSS."),
            ForAllEnabled
        );

        public static readonly FeatureDefinition PublicEditForm = new FeatureDefinition(
            "PublicEditForm",
            new Guid("d93baf71-74c6-4956-9fe0-8281acdfd14a"),
            "Public use of Edit Form",
            Public,
            NotForUi,
            "Allows non-editors to use the Edit-Form to edit/create data. <br>" +
            "This is for commands like @Edit.Enable(...) or certain IPageService.Activate(...)",
            new FeatureSecurity(4, "Could affect security if a bug would allow saving data without permission checks."),
            ForAllDisabled
        );


        public static readonly FeatureDefinition PublicUploadFiles = new FeatureDefinition(
            "PublicUploadFiles",
            new Guid("79b9f5f8-d104-458b-8e8f-9f4a11c5935e"),
            "Public Upload of Files",
            Public,
            NotForUi,
            "Allow non-editors to upload files. <br>" +
            "With this enabled, public forms will allow file-upload with ADAM. Otherwise it will be disabled. <br>" +
            "Note that each of these fields must also have the permissions set allowing an upload. This is to prevent uploads on fields which were just meant to hold a link.",
            new FeatureSecurity(4, "Could affect security if a bug allows saving files with bad extensions."),
            ForAllDisabled
        );

        public static readonly FeatureDefinition SaveInAdamApi = new FeatureDefinition(
            "SaveInAdamApi",
            new Guid("ecdab0f6-4692-4544-b1e7-72581f489f6a"),
            "SaveInAdam API",
            NotForPublic,
            NotForUi,
            "Enable the SaveInAdam command in Razor / C#. Otherwise, SaveInAdam(...) will be refused, and custom WebAPIs trying to do this (like Mobius forms with Upload) will throw an error.",
            new FeatureSecurity(2, "Could affect security if a bug allows saving files with bad extensions."),
            ForAllEnabled
        );

        public static readonly FeatureDefinition PermissionCheckUsers = new FeatureDefinition(
            "PermissionCheckUsers",
            new Guid("47c71ee9-ac7b-45bf-a08b-dfc8ce7c7775"),
            "Permission Check based on User",
            NotForPublic,
            NotForUi,
            "Enable permission checks by user (User ID)",
            new FeatureSecurity(1, "Could affect security if a bug in security checks doesn't correctly match IDs."),
            ForAllDisabled
        );

        public static readonly FeatureDefinition PermissionCheckGroups = new FeatureDefinition(
            "PermissionCheckGroups",
            new Guid("0fd479cc-300f-47fd-88fd-8f2fe092bc09"),
            "Permission Check based on Group",
            NotForPublic,
            NotForUi,
            "Enable permission checks to use the Groups a user belongs to based on the Group/Role-ID",
            new FeatureSecurity(1, "Could affect security if a bug in security checks doesn't correctly match IDs."),
            ForAllDisabled
        );

        public static readonly FeatureDefinition EditUiShowNotes = new FeatureDefinition(
            "EditUiShowNotes",
            new Guid("945320af-9ba9-4117-87cb-d63815e99fd4"),
            "Edit UI: Show notes button",
            Public,
            ForUi,
            "",
            FeaturesCatalogRules.Security0Neutral,
            ForAllEnabled
        );

        public static readonly FeatureDefinition EditUiShowMetadataFor = new FeatureDefinition(
            "EditUiShowMetadataFor",
            new Guid("717b5d0a-07b1-41ec-a670-ec9665cd4af1"),
            "Edit UI: Show information if something is Metadata-For",
            Public,
            ForUi,
            "",
            FeaturesCatalogRules.Security0Neutral,
            ForAllEnabled
        );
    }
}
