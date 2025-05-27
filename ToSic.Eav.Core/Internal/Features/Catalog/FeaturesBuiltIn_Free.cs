using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.SysData;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Internal.Features;

public partial class BuiltInFeatures
{

    public const bool ForUi = true;
    public const bool NotForUi = false;
    public const bool Public = true;
    public const bool NotForPublic = false;

    public static readonly Feature WysiwygPasteFormatted = new()
    {
        NameId = "WysiwygPasteFormatted",
        Guid = new("1b13e0e6-a346-4454-a1e6-2fb18c047d20"),
        Name = "Paste Formatted Text",
        IsPublic = Public,
        Ui = ForUi,
        Description = "Paste formatted text into WYSIWYG TinyMCE",
        Security = new(2, "Should not affect security, unless a TinyMCE bug allows adding script tags or similar which could result in XSS."),
        LicenseRules = BuiltInLicenseRules.ForAllEnabled
    };




    public static readonly Feature EditUiShowNotes = new()
    {
        NameId = "EditUiShowNotes",
        Guid = new("945320af-9ba9-4117-87cb-d63815e99fd4"),
        Name = "Edit UI = Show notes button",
        IsPublic = Public,
        Ui = ForUi,
        Description = "",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = BuiltInLicenseRules.ForAllEnabled
    };

    public static readonly Feature EditUiShowMetadataFor = new()
    {
        NameId = "EditUiShowMetadataFor",
        Guid = new("717b5d0a-07b1-41ec-a670-ec9665cd4af1"),
        Name = "Edit UI = Show information if something is Metadata-For",
        IsPublic = Public,
        Ui = ForUi,
        Description = "",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = BuiltInLicenseRules.ForAllEnabled
    };

    public static readonly Feature InsightsLoggingCustomized = new()
    {
        NameId = nameof(InsightsLoggingCustomized),
        Guid = new("2a74b68e-0fd7-404a-9bcb-8fdab272fd48"),
        Name = "Logging - Custom Configuration",
        IsPublic = NotForPublic,
        Ui = NotForUi,
        Description = "Configure server logging - usually to take less memory.",
        Security = new(1, "In some cases you won't see what happened previously."),
        LicenseRules = BuiltInLicenseRules.ForAllEnabled,
        ConfigurationContentType = "5d70d9f6-3c03-4c09-8413-1a0c2e8dbf8f",
    };

    /// <summary>
    /// Dummy class to manage the keys we want to read of the configuration
    /// </summary>
    public class InsightsLoggingCustomConfig
    {
        public bool LoadSystemData;
        public bool LoadSystemDataDetails;
        public bool LoadSystemDataSummary;

        public bool LoadApp;
        public bool LoadAppDetails;
        public bool LoadAppSummary;
        public string LoadAppIdCsv;
    }
}