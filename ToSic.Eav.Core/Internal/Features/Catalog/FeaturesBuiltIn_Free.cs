﻿using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features;

public partial class BuiltInFeatures
{
    public static List<FeatureLicenseRule> ForAllEnabled = BuildRule(BuiltInLicenses.CoreFree, true);
    public static List<FeatureLicenseRule> ForAllDisabled = BuildRule(BuiltInLicenses.CoreFree, false);

    public static List<FeatureLicenseRule> SystemEnabled = BuildRule(BuiltInLicenses.System, true);
    public static List<FeatureLicenseRule> ExtensionEnabled = BuildRule(BuiltInLicenses.Extension, true);

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
        LicenseRules = ForAllEnabled
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
        LicenseRules = ForAllEnabled
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
        LicenseRules = ForAllEnabled
    };


}