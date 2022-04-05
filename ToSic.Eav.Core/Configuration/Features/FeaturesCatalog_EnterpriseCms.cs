using System;
using System.Collections.Generic;
using ToSic.Eav.Configuration.Licenses;

namespace ToSic.Eav.Configuration
{
    public partial class FeaturesCatalog
    {
        internal static List<FeatureLicenseRule> ForEnterpriseCms = BuildRule(BuiltIn.EnterpriseCms, true);

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
    }
}
