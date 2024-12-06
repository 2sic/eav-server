using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features;

public partial class BuiltInFeatures
{
    internal static List<FeatureLicenseRule> ForPatronLanguages = BuildRule(BuiltInLicenses.PatronLanguages, true);


    public static readonly Feature EditUiTranslateWithGoogle = new()
    {
        NameId = nameof(EditUiTranslateWithGoogle),
        Guid = new("353186b4-7e19-41fb-9dca-c408c26e43d7"),
        Name = "Edit UI - Enable Translate with Google Translate",
        IsPublic = true,
        Ui = true,
        Description = "Allow editors to translate the content using Google Translate. Important = Requires a Google Translate Key and some initial setup.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronLanguages,
    
        DisabledBehavior = FeatureDisabledBehavior.Nag,
    };

    public static readonly Feature LanguagesAdvancedFallback = new()
    {
        NameId = nameof(LanguagesAdvancedFallback),
        Guid = new("95bb2232-ec19-4f9c-adf9-9df07d841cc8"),
        Name = "Languages - Customize language fallback sequence",
        IsPublic = true,
        Ui = false,
        Description = "Allow admins so specify language fallback at a granular level.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronLanguages,
    };

}