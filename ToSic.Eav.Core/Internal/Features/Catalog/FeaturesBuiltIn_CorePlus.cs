using System;
using System.Collections.Generic;
using ToSic.Eav.Configuration.Licenses;

namespace ToSic.Eav.Configuration
{
    public partial class BuiltInFeatures
    {
        public static List<FeatureLicenseRule> ForCorePlusEnabled = BuildRule(BuiltInLicenses.CorePlus, true);
        public static List<FeatureLicenseRule> ForCorePlusDisabled = BuildRule(BuiltInLicenses.CorePlus, false);


        public static readonly FeatureDefinition PublicEditForm = new FeatureDefinition(
            "PublicEditForm",
            new Guid("d93baf71-74c6-4956-9fe0-8281acdfd14a"),
            "Public use of Edit Form",
            Public,
            NotForUi,
            "Allows non-editors to use the Edit-Form to edit/create data. <br>" +
            "This is for commands like @Edit.Enable(...) or certain IPageService.Activate(...)",
            new FeatureSecurity(4, "Could affect security if a bug would allow saving data without permission checks."),
            ForCorePlusDisabled
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
            ForCorePlusDisabled
        );

        public static readonly FeatureDefinition SaveInAdamApi = new FeatureDefinition(
            "SaveInAdamApi",
            new Guid("ecdab0f6-4692-4544-b1e7-72581f489f6a"),
            "SaveInAdam API",
            NotForPublic,
            NotForUi,
            "Enable the SaveInAdam command in Razor / C#. Otherwise, SaveInAdam(...) will be refused, and custom WebAPIs trying to do this (like Mobius forms with Upload) will throw an error.",
            new FeatureSecurity(2, "Could affect security if a bug allows saving files with bad extensions."),
            ForCorePlusEnabled
        );

        public static readonly FeatureDefinition PermissionCheckUsers = new FeatureDefinition(
            "PermissionCheckUsers",
            new Guid("47c71ee9-ac7b-45bf-a08b-dfc8ce7c7775"),
            "Permission Check based on User",
            NotForPublic,
            NotForUi,
            "Enable permission checks by user (User ID)",
            new FeatureSecurity(1, "Could affect security if a bug in security checks doesn't correctly match IDs."),
            ForCorePlusEnabled
        );

        public static readonly FeatureDefinition PermissionCheckGroups = new FeatureDefinition(
            "PermissionCheckGroups",
            new Guid("0fd479cc-300f-47fd-88fd-8f2fe092bc09"),
            "Permission Check based on Group",
            NotForPublic,
            NotForUi,
            "Enable permission checks to use the Groups a user belongs to based on the Group/Role-ID",
            new FeatureSecurity(1, "Could affect security if a bug in security checks doesn't correctly match IDs."),
            ForCorePlusEnabled
        );

        public static readonly FeatureDefinition EditUiAllowDebugModeForEditors = new FeatureDefinition(
            "EditUiAllowDebugModeForEditors",
            new Guid("a7703dbe-2659-44c7-9ef5-1cd114357d86"),
            "Edit UI: Allow normal editors (not System-Admins) to enter Debug-Mode",
            Public,
            ForUi,
            "",
            new FeatureSecurity(2, "Reduces security. should only be activated during development or for special debugging. "),
            ForCorePlusDisabled
        );

    }
}
