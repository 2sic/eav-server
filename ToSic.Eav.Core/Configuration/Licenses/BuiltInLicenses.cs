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

namespace ToSic.Eav.Configuration.Licenses
{
    public class BuiltInLicenses
    {

        // IMPORTANT
        // The guids of these licenses must match the ones in the 2sxc.org license management list
        // So always create the definition there first, then use the GUID of that definition here

        public static readonly DateTime UnlimitedExpiry = DateTime.MaxValue;

        public static readonly LicenseDefinition CoreFree = new LicenseDefinition(1, 
            "Core (free for everyone)",
            new Guid("40e49a48-0bcd-429c-b6b1-a21e05886bdf"),
            "The core, free, open-source license covers 99% of all features in 2sxc. Most of the features are not even listed, as they are always enabled."
        )
        {
            AutoEnable = true
        };

        public static readonly LicenseDefinition CorePlus = new LicenseDefinition(2,
            "Core+ (free for everyone who registers)",
            new Guid("86376be0-b06f-4f6f-884b-cce80a456327"),
            "These core features are free for anyone who registers their system. It has various features which may be security relevant, so by registering we can inform you if security issues appear. "
        );

        public static readonly LicenseDefinition PatronPerfectionist = new LicenseDefinition(121, 
            "Patron Perfectionist",
            new Guid("015077bb-9829-4291-bf99-244d8ba3b100"),
            "Patrons / supporters of 2sxc who really care about perfect pictures and ultra-fast caching. They get some very enhanced goodies."
        );

        public static readonly LicenseDefinition PatronSentinel = new LicenseDefinition(122, 
            "Patron Sentinel",
            new Guid("f1e00b9c-1363-4cf6-a8bc-60a215a4130a"),
            "Patrons / supporters of 2sxc who really care extra high security such as CSP (Content Security Policies) and other features."
        );


        public static readonly LicenseDefinition PatronBasic = new LicenseDefinition(101, 
            "Patron Basic",
            new Guid("61d0bf11-187c-4ae8-9b76-a2c3d4beaad7"),
            "Patrons / supporters of 2sxc get some additional features as a thank you for supporting 2sxc."
        )
        {
            // Beta, functionality not implemented
            AlsoInheritEnabledFrom = new [] { PatronPerfectionist }
        };

        public static readonly LicenseDefinition PatronAdvanced = new LicenseDefinition(102,
            "Patron Advanced",
            new Guid("e23ef849-f50c-47a5-81dd-33fb17727305"),
            "Patrons with advanced needs in regards to automation, administration etc."
        );


        public static readonly LicenseDefinition WebFarmCache = new LicenseDefinition(301, 
            "WebFarmCache",
            new Guid("ed5ca2e7-4c13-422c-ad8f-e47e78e3d0d9"),
            "DNN installations which run as a Farm can enable WebFarm caching to ensure the servers are in sync when something is edited."
        );

        public static readonly LicenseDefinition EnterpriseCms = new LicenseDefinition(202, 
            "EnterpriseCms",
            new Guid("da7274c1-b893-4edb-8acb-ae2995a07321"),
            "Extreme CMS features for complex sites with extra needs, like global, shared Apps across many portals. "
        );

        public static readonly LicenseDefinition CoreBeta = new LicenseDefinition(1000,
            "Beta",
            new Guid("a96277f0-df0e-4dc2-a9a6-4951fb43f26f"),
            "This enables beta features in 2sxc which are not yet final and may change at any time."
        );
    }
}
