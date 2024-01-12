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

using System;
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

    public static readonly FeatureSet System = new(FeatureSetSystem, 10000,
        "System",
        new("fae8a2ac-cdeb-45f8-b690-cc4eee8a5690"),
        "System features which are provided by the platform you are running on or installed as a system-feature (eg. Compiler). All the features should begin with \"System-\""
    )
    {
        AutoEnable = true
    };
    public static readonly FeatureSet Extension = new(FeatureSetExtension, 10001,
        "Extension",
        new("8d0cef7a-1e11-456e-aae5-94ac21c0cc74"),
        "Extensions / Plugins which were installed on your system. All the feature names should begin with \"Extension-\""
    )
    {
        AutoEnable = true
    };

    public static readonly FeatureSet CoreFree = new(LicensePrefix + nameof(CoreFree), 1, 
        "Core (free for everyone)",
        new("40e49a48-0bcd-429c-b6b1-a21e05886bdf"),
        "The core, free, open-source license covers 99% of all features in 2sxc. Most of the features are not even listed, as they are always enabled."
    )
    {
        AutoEnable = true
    };

    public static readonly FeatureSet CorePlus = new(LicensePrefix + nameof(CorePlus), 2,
        "Core+ (free for everyone who registers)",
        new("86376be0-b06f-4f6f-884b-cce80a456327"),
        "These core features are free for anyone who registers their system. It has various features which may be security relevant, so by registering we can inform you if security issues appear. "
    );

    public static readonly FeatureSet PatronPerfectionist = new(LicensePrefix + nameof(PatronPerfectionist),121, 
        "Patron Perfectionist",
        new("015077bb-9829-4291-bf99-244d8ba3b100"),
        "Patrons / supporters of 2sxc who really care about perfect pictures and ultra-fast caching. They get some very enhanced goodies."
    );

    public static readonly FeatureSet PatronSentinel = new(LicensePrefix + nameof(PatronSentinel),122, 
        "Patron Sentinel",
        new("f1e00b9c-1363-4cf6-a8bc-60a215a4130a"),
        "Patrons / supporters of 2sxc who really care extra high security such as CSP (Content Security Policies) and other features."
    );


    public static readonly FeatureSet PatronBasic = new(LicensePrefix + nameof(PatronBasic),101, 
        "Patron Basic",
        new("61d0bf11-187c-4ae8-9b76-a2c3d4beaad7"),
        "Patrons / supporters of 2sxc get some additional features as a thank you for supporting 2sxc."
    )
    {
        // Beta, functionality not implemented
        AlsoInheritEnabledFrom = new [] { PatronPerfectionist }
    };

    public static readonly FeatureSet PatronAdvancedCms = new(LicensePrefix + nameof(PatronAdvancedCms),102,
        "Patron Advanced CMS",
        new("e23ef849-f50c-47a5-81dd-33fb17727305"),
        "Patrons with advanced needs in advanced CMS features."
    );

    public static readonly FeatureSet PatronSuperAdmin = new(LicensePrefix + nameof(PatronSuperAdmin),501,
        "Patron SuperAdmin",
        new("4f1aa6d9-6360-4c60-97e9-643e3fd92061"),
        "Patrons with advanced needs in regards to automation, administration etc."
    );

    public static readonly FeatureSet PatronInfrastructure = new(LicensePrefix + nameof(PatronInfrastructure),501,
        "Patron Infrastructure",
        new("68b2c253-25b9-4b4f-b9d5-f95fd2ef9d75"),
        "Patrons who wish to save cost by optimizing their infrastructure."
    );

    public static readonly FeatureSet WebFarmCache = new(LicensePrefix + nameof(WebFarmCache),301, 
        "WebFarmCache",
        new("ed5ca2e7-4c13-422c-ad8f-e47e78e3d0d9"),
        "DNN installations which run as a Farm can enable WebFarm caching to ensure the servers are in sync when something is edited."
    );

    public static readonly FeatureSet EnterpriseCms = new(LicensePrefix + nameof(EnterpriseCms),702, 
        "EnterpriseCms",
        new("da7274c1-b893-4edb-8acb-ae2995a07321"),
        "Extreme CMS features for complex sites with extra needs, like global, shared Apps across many portals. "
    );

    public static readonly FeatureSet CoreBeta = new(LicensePrefix + nameof(CoreBeta),1000,
        "Beta",
        new("a96277f0-df0e-4dc2-a9a6-4951fb43f26f"),
        "This enables beta features in 2sxc which are not yet final and may change at any time."
    );

#if DEBUG
    public static readonly FeatureSet CoreTesting = new(LicensePrefix + nameof(CoreTesting), TestLicensesBaseId,
        "Testing",
        new("5c7b019d-3289-4d6e-bda1-6c165d3fa1e0"),
        "This is just for testing - it doesn't do anything."
    );
#endif
}