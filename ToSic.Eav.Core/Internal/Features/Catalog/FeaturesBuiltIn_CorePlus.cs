using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features;

public partial class BuiltInFeatures
{
    public static List<FeatureLicenseRule> ForCorePlusEnabled = BuildRule(licDef: BuiltInLicenses.CorePlus, featureEnabled: true);
    public static List<FeatureLicenseRule> ForCorePlusDisabled = BuildRule(licDef: BuiltInLicenses.CorePlus, featureEnabled: false);


    public static readonly Feature PublicEditForm = new() {
        NameId = "PublicEditForm",
        Guid = new("d93baf71-74c6-4956-9fe0-8281acdfd14a"),
        Name = "Public use of Edit Form",
        IsPublic = Public,
        Ui = NotForUi,
        Description = "Allows non-editors to use the Edit-Form to edit/create data. <br>" +
                     "This is for commands like @Edit.Enable(...) or certain IPageService.Activate(...)",
        Security = new(4, "Could affect security if a bug would allow saving data without permission checks."),
        LicenseRules = ForCorePlusDisabled
    };


    public static readonly Feature PublicUploadFiles = new() {
        NameId = "PublicUploadFiles",
        Guid = new("79b9f5f8-d104-458b-8e8f-9f4a11c5935e"),
        Name = "Public Upload of Files",
        IsPublic = Public,
        Ui = NotForUi,
        Description = "Allow non-editors to upload files. <br>" +
                     "With this enabled, public forms will allow file-upload with ADAM. Otherwise it will be disabled. <br>" +
                     "Note that each of these fields must also have the permissions set allowing an upload. This is to prevent uploads on fields which were just meant to hold a link.",
        Security = new(4, "Could affect security if a bug allows saving files with bad extensions."),
        LicenseRules = ForCorePlusDisabled
    };

    public static readonly Feature SaveInAdamApi = new() {
        NameId = nameof(SaveInAdamApi),
        Guid = new("ecdab0f6-4692-4544-b1e7-72581f489f6a"),
        Name = "SaveInAdam API",
        IsPublic = NotForPublic,
        Ui = NotForUi,
        Description = "Enable the SaveInAdam command in Razor / C#. Otherwise, SaveInAdam(...) will be refused, and custom WebAPIs trying to do this (like Mobius forms with Upload) will throw an error.",
        Security = new(2, "Could affect security if a bug allows saving files with bad extensions."),
        LicenseRules = ForCorePlusEnabled
    };

    public static readonly Feature PermissionCheckUsers = new() {
        NameId = nameof(PermissionCheckUsers),
        Guid = new("47c71ee9-ac7b-45bf-a08b-dfc8ce7c7775"),
        Name = "Permission Check based on User",
        IsPublic = NotForPublic,
        Ui = NotForUi,
        Description = "Enable permission checks by user (User ID)",
        Security = new(1, "Could affect security if a bug in security checks doesn't correctly match IDs."),
        LicenseRules = ForCorePlusEnabled
    };

    public static readonly Feature PermissionCheckGroups = new() {
        NameId = nameof(PermissionCheckGroups),
        Guid = new("0fd479cc-300f-47fd-88fd-8f2fe092bc09"),
        Name = "Permission Check based on Group",
        IsPublic = NotForPublic,
        Ui = NotForUi,
        Description = "Enable permission checks to use the Groups a user belongs to based on the Group/Role-ID",
        Security = new(1, "Could affect security if a bug in security checks doesn't correctly match IDs."),
        LicenseRules = ForCorePlusEnabled
    };

    public static readonly Feature EditUiAllowDebugModeForEditors = new() {
        NameId = "EditUiAllowDebugModeForEditors",
        Guid = new("a7703dbe-2659-44c7-9ef5-1cd114357d86"),
        Name = "Edit UI = Allow normal editors (not System-Admins) to enter Debug-Mode",
        IsPublic = Public,
        Ui = ForUi,
        Description = "",
        Security = new(2, "Reduces security. should only be activated during development or for special debugging. "),
        LicenseRules = ForCorePlusDisabled
    };

}