using System;
using System.Collections.Generic;

namespace ToSic.Eav.Configuration
{
    public partial class BuiltInFeatures
    {
        internal static List<FeatureLicenseRule> ForEnterpriseCms = BuildRule(Licenses.BuiltInLicenses.EnterpriseCms, true);
        internal static List<FeatureLicenseRule> ForEnterpriseCmsDisabled = BuildRule(Licenses.BuiltInLicenses.EnterpriseCms, false);

        // WIP / Beta in v13
        public static readonly FeatureDefinition SharedApps = new FeatureDefinition(
            "SharedApps",
            new Guid("bb6656ef-fb81-4943-bf88-297e516d2616"),
            "Share Apps to Reuse on Many Sites",
            false,
            false,
            "Allows you to define shared global Apps which can be inherited and re-used on many Sites.", FeaturesCatalogRules.Security0Improved,
            ForEnterpriseCms
        );

        public static readonly FeatureDefinition PermissionsByLanguage = new FeatureDefinition(
            "PermissionsByLanguage",
            new Guid("fc1efaaa-89a0-446d-83de-89e20b3ce0d7"),
            "Edit-Permissions by Language",
            false,
            false,
            "Configure who can edit what language in the Edit UI.", FeaturesCatalogRules.Security0Improved,
            ForEnterpriseCms
        );

        public static readonly FeatureDefinition EditUiDisableDraft = new FeatureDefinition(
            "EditUiDisableDraft",
            new Guid("09cc2d62-e640-49dc-a267-2312aff97f55"),
            "Edit-UI - Disable draft mode",
            false,
            false,
            "Completely disable draft-mode in the Edit UI.", FeaturesCatalogRules.Security0Improved,
            ForEnterpriseCmsDisabled
        );
    }
}
