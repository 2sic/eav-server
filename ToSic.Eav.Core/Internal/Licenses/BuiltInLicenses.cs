/*
 * Copyright 2022 by 2sic internet solutions in Switzerland - www.2sic.com
 *
 * This file and the code IS COPYRIGHTED.
 * 1. You may not change it.
 * 2. You may not copy the code to reuse in another way.
 *
 * Copying this or creating a similar service, 
 * especially when used to circumvent licensing features in EAV and 2sxc
 * is a copyright infringement.
 *
 * Please remember that 2sic has sponsored more than 10 years of work,
 * and paid more than 1 Million USD in wages for its development.
 * So asking for support to finance advanced features is not asking for much. 
 *
 */

using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Licenses;

public class BuiltInLicenses
{
    public const int TestLicensesBaseId = 9000;
    public const int FeatureLicensesBaseId = 9900;
    public const string LicensePrefix = "License-";
    public const string LicenseCustom = LicensePrefix + "Custom";
    public const string FeatureSetSystem = "System";   // Feature Set "System"
    public const string FeatureSetExtension = "Extension";

    // IMPORTANT
    // The guids of these licenses must match the ones in the 2sxc.org license management list
    // So always create the definition there first, then use the GUID of that definition here

    public static readonly DateTime UnlimitedExpiry = DateTime.MaxValue;

    public static readonly FeatureSet System = new()
    {
        NameId = FeatureSetSystem, Priority = 10000,
        Name = "System",
        Guid = new("fae8a2ac-cdeb-45f8-b690-cc4eee8a5690"),
        Description = "System features which are provided by the platform you are running on or installed as a system-feature (like Compiler). All the features should begin with \"System-\"",

        AutoEnable = true
    };

    public static readonly FeatureSet Extension = new()
    {
        NameId = FeatureSetExtension, Priority = 10001,
        Name = "Extension",
        Guid = new("8d0cef7a-1e11-456e-aae5-94ac21c0cc74"),
        Description = "Extensions / Plugins which were installed on your system. All the feature names should begin with \"Extension-\"",

        AutoEnable = true
    };

    public static readonly FeatureSet CoreFree = new()
    {
        NameId = LicensePrefix + nameof(CoreFree), Priority = 1,
        Name = "Core (free for everyone)",
        Guid = new("40e49a48-0bcd-429c-b6b1-a21e05886bdf"),
        Description = "The core, free, open-source license covers 99% of all features in 2sxc. Most of the features are not even listed, as they are always enabled.",

        AutoEnable = true
    };

    public static readonly FeatureSet CorePlus = new()
    {
        NameId = LicensePrefix + nameof(CorePlus), Priority = 2,
        Name = "Core+ (free for everyone who registers)",
        Guid = new("86376be0-b06f-4f6f-884b-cce80a456327"),
        Description = "These core features are free for anyone who registers their system. It has various features which may be security relevant, so by registering we can inform you if security issues appear. ",
    };

    public static readonly FeatureSet PatronPerfectionist = new()
    {
        NameId = LicensePrefix + nameof(PatronPerfectionist), Priority = 121,
        Name = "Patron Perfectionist",
        Guid = new("015077bb-9829-4291-bf99-244d8ba3b100"),
        Description = "Patrons / supporters of 2sxc who really care about perfect pictures and ultra-fast caching. They get some very enhanced goodies."
    };

    public static readonly FeatureSet PatronSentinel = new()
    {
        NameId = LicensePrefix + nameof(PatronSentinel), Priority = 122,
        Name = "Patron Sentinel",
        Guid = new("f1e00b9c-1363-4cf6-a8bc-60a215a4130a"),
        Description = "Patrons / supporters of 2sxc who really care extra high security such as CSP (Content Security Policies) and other features."
    };


    public static readonly FeatureSet PatronBasic = new()
    {
        NameId = LicensePrefix + nameof(PatronBasic), Priority = 101,
        Name = "Patron Basic",
        Guid = new("61d0bf11-187c-4ae8-9b76-a2c3d4beaad7"),
        Description = "Patrons / supporters of 2sxc get some additional features as a thank you for supporting 2sxc.",

        // Beta, functionality not implemented
        AlsoInheritEnabledFrom = [PatronPerfectionist]
    };

    public static readonly FeatureSet PatronAdvancedCms = new()
    {
        NameId = LicensePrefix + nameof(PatronAdvancedCms), Priority = 102,
        Name = "Patron Advanced CMS",
        Guid = new("4df6895d-2ec4-4fcd-ae2a-7f49defb584b"),
        Description = "Patrons with advanced needs in advanced CMS features."
    };

    /// <summary>
    /// Languages.
    /// Note that previously the GUID was used for Patrons Advanced, but as we modified this in 18.02
    /// we decided that the features previously in PatronAdvanced are usually licensed for languages, not for the other features (Copyright).
    /// </summary>
    public static readonly FeatureSet PatronLanguages = new()
    {
        NameId = LicensePrefix + nameof(PatronLanguages), Priority = 102,
        Name = "Patron Languages",
        Guid = new("e23ef849-f50c-47a5-81dd-33fb17727305"),
        Description = "Patrons with advanced needs in managing languages."
    };

    public static readonly FeatureSet PatronData = new()
    {
        NameId = LicensePrefix + nameof(PatronData), Priority = 102,
        Name = "Patron Data",
        Guid = new("5a50b61e-65ff-443f-a984-a9656e51eb20"),
        Description = "Awesome data features such as content-type inheritance and advanced pickers."
    };

    public static readonly FeatureSet PatronSuperAdmin = new()
    {
        NameId = LicensePrefix + nameof(PatronSuperAdmin), Priority = 501,
        Name = "Patron SuperAdmin",
        Guid = new("4f1aa6d9-6360-4c60-97e9-643e3fd92061"),
        Description = "Patrons with advanced needs in regards to automation, administration etc."
    };

    public static readonly FeatureSet PatronInfrastructure = new()
    {
        NameId = LicensePrefix + nameof(PatronInfrastructure), Priority = 501,
        Name = "Patron Infrastructure",
        Guid = new("68b2c253-25b9-4b4f-b9d5-f95fd2ef9d75"),
        Description = "Patrons who wish to save cost by optimizing their infrastructure."
    };

    public static readonly FeatureSet WebFarmCache = new()
    {
        NameId = LicensePrefix + nameof(WebFarmCache), Priority = 301,
        Name = "WebFarmCache",
        Guid = new("ed5ca2e7-4c13-422c-ad8f-e47e78e3d0d9"),
        Description =
            "DNN installations which run as a Farm can enable WebFarm caching to ensure the servers are in sync when something is edited."
    };

    public static readonly FeatureSet EnterpriseCms = new()
    {
        NameId = LicensePrefix + nameof(EnterpriseCms), Priority = 702,
        Name = "EnterpriseCms",
        Guid = new("da7274c1-b893-4edb-8acb-ae2995a07321"),
        Description =
            "Extreme CMS features for complex sites with extra needs, like global, shared Apps across many portals. "
    };

    public static readonly FeatureSet CoreBeta = new()
    {
        NameId = LicensePrefix + nameof(CoreBeta), Priority = 1000,
        Name = "Beta",
        Guid = new("a96277f0-df0e-4dc2-a9a6-4951fb43f26f"),
        Description = "This enables beta features in 2sxc which are not yet final and may change at any time."
    };

#if DEBUG
    public static readonly FeatureSet CoreTesting = new()
    {
        NameId = LicensePrefix + nameof(CoreTesting), Priority = TestLicensesBaseId,
        Name = "Testing",
        Guid = new("5c7b019d-3289-4d6e-bda1-6c165d3fa1e0"),
        Description = "This is just for testing - it doesn't do anything."
    };
#endif
}