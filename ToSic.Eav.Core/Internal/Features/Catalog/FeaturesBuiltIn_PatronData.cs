using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Capabilities.Licenses;

namespace ToSic.Eav.Internal.Features;

public partial class BuiltInFeatures
{
    internal static List<FeatureLicenseRule> ForPatronDataAutoEnabled = BuiltInLicenseRules.BuildRule(BuiltInLicenses.PatronData, true);


    public static readonly Feature ContentTypeFieldsReuseDefinitions = new(){
        NameId = nameof(ContentTypeFieldsReuseDefinitions),
        Guid = new("e0398b1f-32ca-4734-b49a-83ff894e352e"),
        Name = "Content Type - Fields - Share Definitions (for Type-Inheritance)",
        IsPublic = false,
        Ui = true,
        Description = "Use Field Configuration/Metadata Sharing.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronDataAutoEnabled,

        EnableForSystemTypes = true,
        DisabledBehavior = FeatureDisabledBehavior.Warn,
    };

    public static readonly Feature PickerUiCheckbox = new() {
        NameId = nameof(PickerUiCheckbox),
        Guid = new("620c1a1d-5724-40af-a23a-15f2812acbb6"),
        Name = "Picker UI with Checkboxes",
        IsPublic = false,
        Ui = true,
        Description = "Enables the UI to use checkboxes for selecting values, instead of just dropdowns.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronDataAutoEnabled,
     
        EnableForSystemTypes = true,
        DisabledBehavior = FeatureDisabledBehavior.Warn,
    };

    public static readonly Feature PickerUiRadio = new() {
        NameId = nameof(PickerUiRadio),
        Guid = new("2143351b-2997-476e-b9e3-5b792e05c2a4"),
        Name = "Picker UI with Radio Buttons",
        IsPublic = false,
        Ui = true,
        Description = "Enables the UI to use radio buttons for selecting values, instead of just dropdowns.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronDataAutoEnabled,


        EnableForSystemTypes = true,
        DisabledBehavior = FeatureDisabledBehavior.Warn,
    };

    public static readonly Feature PickerSourceCsv = new() {
        NameId = nameof(PickerSourceCsv),
        Guid = new("2079c3c0-fb11-40e9-b3f2-53a70e8cea10"),
        Name = "Picker source using CSV table",
        IsPublic = false,
        Ui = true,
        Description = "Allows developers to use simple CSV tables as a data-source for pickers. This is especially useful to add more info, links, tooltips etc.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronDataAutoEnabled,
    
        EnableForSystemTypes = true,
        DisabledBehavior = FeatureDisabledBehavior.Warn,
    };

    public static readonly Feature PickerSourceAppAssets = new() {
        NameId = nameof(PickerSourceAppAssets),
        Guid = new("4cb850ef-224f-4eb3-b00c-b14c605e5b29"),
        Name = "Picker source App Files and Folders",
        IsPublic = false,
        Ui = true,
        Description = "Picker DataSource to select files and folders in an App. This is great for selecting SVG icons or similar things.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronDataAutoEnabled,
    
        EnableForSystemTypes = true,
        DisabledBehavior = FeatureDisabledBehavior.Warn,
    };

    public static readonly Feature PickerFormulas = new() {
        NameId = nameof(PickerFormulas),
        Guid = new("44da3226-63bc-4da4-a341-c9542e9b4013"),
        Name = "Picker Formulas",
        IsPublic = false,
        Ui = true,
        Description = "Enable the use of formulas with picker fields.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronDataAutoEnabled,
     
        EnableForSystemTypes = true,
        DisabledBehavior = FeatureDisabledBehavior.Warn,
    };

    public static readonly Feature PickerUiMoreInfo = new() {
        NameId = nameof(PickerUiMoreInfo),
        Guid = new("e5014b5a-2b61-47a4-b36a-77cb4f043c5a"),
        Name = "Picker UI More Info",
        IsPublic = false,
        Ui = true,
        Description = "Enable the use of tooltips, links and more with picker fields.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronDataAutoEnabled,

        EnableForSystemTypes = true,
        DisabledBehavior = FeatureDisabledBehavior.Downgrade,
    };

}