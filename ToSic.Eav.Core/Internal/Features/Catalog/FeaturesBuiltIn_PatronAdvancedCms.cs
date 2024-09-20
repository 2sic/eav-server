using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features;

public partial class BuiltInFeatures
{
    internal static List<FeatureLicenseRule> ForPatronAdvancedCmsAutoEnabled = BuildRule(BuiltInLicenses.PatronAdvancedCms, true);


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

    public static readonly Feature FieldShareConfigManagement = new(
        nameof(FieldShareConfigManagement),
        new("e0398b1f-32ca-4734-b49a-83ff894e352e"),
        "Field Sharing - Enable Configure in Admin UI",
        false,
        true,
        "Enable Field Sharing Management directly in the UI.",
        FeaturesCatalogRules.Security0Neutral,
        ForPatronAdvancedCmsAutoEnabled,
        enableForSystemTypes: true,
        disabledBehavior: FeatureDisabledBehavior.Warn
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

    public static readonly Feature PickerUiCheckbox = new(
        nameof(PickerUiCheckbox),
        new("620c1a1d-5724-40af-a23a-15f2812acbb6"),
        "Picker UI with Checkboxes",
        isPublic: false,
        ui: true,
        description: "Enables the UI to use checkboxes for selecting values, instead of just dropdowns.",
        FeaturesCatalogRules.Security0Neutral,
        ForPatronAdvancedCmsAutoEnabled,
        enableForSystemTypes: true,
        disabledBehavior: FeatureDisabledBehavior.Warn
    );

    public static readonly Feature PickerUiRadio = new(
        nameof(PickerUiRadio),
        new("2143351b-2997-476e-b9e3-5b792e05c2a4"),
        "Picker UI with Radio Buttons",
        isPublic: false,
        ui: true,
        description: "Enables the UI to use radio buttons for selecting values, instead of just dropdowns.",
        FeaturesCatalogRules.Security0Neutral,
        ForPatronAdvancedCmsAutoEnabled,
        enableForSystemTypes: true,
        disabledBehavior: FeatureDisabledBehavior.Warn
    );

    public static readonly Feature PickerSourceCsv = new(
        nameof(PickerSourceCsv),
        new("2079c3c0-fb11-40e9-b3f2-53a70e8cea10"),
        "Picker source using CSV table",
        false,
        true,
        "Allows developers to use simple CSV tables as a data-source for pickers.",
        FeaturesCatalogRules.Security0Neutral,
        ForPatronAdvancedCmsAutoEnabled,
        enableForSystemTypes: true,
        disabledBehavior: FeatureDisabledBehavior.Warn
    );

}