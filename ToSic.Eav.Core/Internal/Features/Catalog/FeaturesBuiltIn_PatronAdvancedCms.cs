using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features;

public partial class BuiltInFeatures
{
    internal static List<FeatureLicenseRule> ForPatronAdvancedCmsAutoEnabled = BuildRule(BuiltInLicenses.PatronLanguages, true);


    public static readonly Feature EditUiTranslateWithGoogle = new(
        nameof(EditUiTranslateWithGoogle),
        new("353186b4-7e19-41fb-9dca-c408c26e43d7"),
        "Edit UI - Enable Translate with Google Translate",
        true,
        true,
        "Allow editors to translate the content using Google Translate. Important: Requires a Google Translate Key and some initial setup.",
        FeaturesCatalogRules.Security0Neutral,
        ForPatronAdvancedCmsAutoEnabled,
        disabledBehavior: FeatureDisabledBehavior.Nag
    );
        
    public static readonly Feature LanguagesAdvancedFallback = new(
        nameof(LanguagesAdvancedFallback),
        new("95bb2232-ec19-4f9c-adf9-9df07d841cc8"),
        "Languages - Customize language fallback sequence",
        true,
        false,
        "Allow admins so specify language fallback at a granular level.",
        FeaturesCatalogRules.Security0Neutral,
        ForPatronAdvancedCmsAutoEnabled
    );

    public static readonly Feature CopyrightManagement = new(
        nameof(CopyrightManagement),
        new("2114297a-d1e7-40d2-88d7-e44cd1111bfa"),
        "Copyright Management for Content (WIP/Beta v17)",
        false,
        true,
        "If enabled, Copyright Management will appear in image toolbars and in future do more. ",
        FeaturesCatalogRules.Security0Neutral,
        ForPatronAdvancedCmsAutoEnabled
    );

}