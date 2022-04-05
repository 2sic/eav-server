using System;
using System.Collections.Generic;
using ToSic.Eav.Configuration.Licenses;

namespace ToSic.Eav.Configuration
{
    public partial class FeaturesCatalog
    {
        internal static List<FeatureLicenseRule> ForBeta = BuildRule(BuiltIn.CoreBeta, false);
        internal static List<FeatureLicenseRule> ForBetaEnabled = BuildRule(BuiltIn.CoreBeta, true);


        // TODO: Probably change how this setting is used, so it's "Enable..." and defaults to true ?
        // ATM only used in azing, so still easy to change

        public static readonly FeatureDefinition BlockFileResolveOutsideOfEntityAdam = new FeatureDefinition(
            "BlockFileResolveOutsideOfEntityAdam-BETA",
            new Guid("702f694c-53bd-4d03-b75c-4dad9c4fb852"),
            "BlockFileResolveOutsideOfEntityAdam (BETA, not final)",
            false,
            false,
            "If enabled, then links like 'file:72' will only work if the file is inside the ADAM of the current Entity.",
            FeaturesCatalogRules.Security0Improved,
            ForBeta
        );

        public static readonly FeatureDefinition RazorThrowPartial = new FeatureDefinition(
            "RazorThrowPartial",
            new Guid("d5a327c5-db0f-472b-93b2-94e66b15e16b"),
            "Will render most of the page and only error on a partial-render, instead of breaking the entire module. ",
            false,
            false,
            "If enabled, then Html.Render or similar activities which throw an error won't stop the entire module, but just that part. ",
            FeaturesCatalogRules.Security0Neutral,
            ForBeta
        );

        public static readonly FeatureDefinition RenderThrowPartialSystemAdmin = new FeatureDefinition(
            "RenderThrowPartialSystemAdmin",
            new Guid("5b0c9379-2fef-4f6e-9022-4d3c50e894e5"),
            "Will render most of the page and only error on a partial-render, instead of breaking the entire module. But only when the sys-admin is viewing the page.",
            false,
            false,
            "If enabled, then Html.Render or similar activities which throw an error won't stop the entire module, but just that part. ",
            FeaturesCatalogRules.Security0Neutral,
            ForBetaEnabled
        );

    }
}
