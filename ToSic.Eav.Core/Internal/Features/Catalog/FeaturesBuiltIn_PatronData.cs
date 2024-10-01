using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features;

public partial class BuiltInFeatures
{
    internal static List<FeatureLicenseRule> ForPatronDataAutoEnabled = BuildRule(BuiltInLicenses.PatronData, true);


    public static readonly Feature ContentTypeFieldsReuseDefinitions = new(
        nameof(ContentTypeFieldsReuseDefinitions),
        new("e0398b1f-32ca-4734-b49a-83ff894e352e"),
        "Content Type - Fields - Share Definitions (for Type-Inheritance)",
        false,
        true,
        "Enable Field Sharing Management directly in the UI.",
        FeaturesCatalogRules.Security0Neutral,
        ForPatronDataAutoEnabled,
        enableForSystemTypes: true,
        disabledBehavior: FeatureDisabledBehavior.Warn
    );

    public static readonly Feature PickerUiCheckbox = new(
        nameof(PickerUiCheckbox),
        new("620c1a1d-5724-40af-a23a-15f2812acbb6"),
        "Picker UI with Checkboxes",
        isPublic: false,
        ui: true,
        description: "Enables the UI to use checkboxes for selecting values, instead of just dropdowns.",
        FeaturesCatalogRules.Security0Neutral,
        ForPatronDataAutoEnabled,
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
        ForPatronDataAutoEnabled,
        enableForSystemTypes: true,
        disabledBehavior: FeatureDisabledBehavior.Warn
    );

    public static readonly Feature PickerSourceCsv = new(
        nameof(PickerSourceCsv),
        new("2079c3c0-fb11-40e9-b3f2-53a70e8cea10"),
        "Picker source using CSV table",
        false,
        true,
        "Allows developers to use simple CSV tables as a data-source for pickers. This is especially useful to add more info, links, tooltips etc.",
        FeaturesCatalogRules.Security0Neutral,
        ForPatronDataAutoEnabled,
        enableForSystemTypes: true,
        disabledBehavior: FeatureDisabledBehavior.Warn
    );

    public static readonly Feature PickerSourceAppAssets = new(
        nameof(PickerSourceAppAssets),
        new("4cb850ef-224f-4eb3-b00c-b14c605e5b29"),
        "Picker source App Files and Folders",
        false,
        true,
        "Picker DataSource to select files and folders in an App. This is great for selecting SVG icons or similar things.",
        FeaturesCatalogRules.Security0Neutral,
        ForPatronDataAutoEnabled,
        enableForSystemTypes: true,
        disabledBehavior: FeatureDisabledBehavior.Warn
    );

    public static readonly Feature PickerFormulas = new(
        nameof(PickerFormulas),
        new("44da3226-63bc-4da4-a341-c9542e9b4013"),
        "Picker Formulas",
        false,
        true,
        "Enable the use of formulas with picker fields.",
        FeaturesCatalogRules.Security0Neutral,
        ForPatronDataAutoEnabled,
        enableForSystemTypes: true,
        disabledBehavior: FeatureDisabledBehavior.Warn
    );

    public static readonly Feature PickerUiMoreInfo = new(
        nameof(PickerUiMoreInfo),
        new("e5014b5a-2b61-47a4-b36a-77cb4f043c5a"),
        "Picker UI More Info",
        false,
        true,
        "Enable the use of tooltips, links and more with picker fields.",
        FeaturesCatalogRules.Security0Neutral,
        ForPatronDataAutoEnabled,
        enableForSystemTypes: true,
        disabledBehavior: FeatureDisabledBehavior.Downgrade
    );

}