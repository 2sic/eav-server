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
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Configuration.Licenses
{
    internal class LicenseCatalog
    {
        // IMPORTANT
        // The guids of these licenses must match the ones in the 2sxc.org license management list
        // So always create the definition there first, then use the GUID of that definition here

        public static readonly DateTime UnlimitedExpiry = new DateTime(2099, 12, 31);

        public static IReadOnlyCollection<LicenseDefinition> Licenses => _licenseTypes ?? (_licenseTypes = CreateList());
        private static IReadOnlyCollection<LicenseDefinition> _licenseTypes;

        public static readonly LicenseDefinition CoreFree = new LicenseDefinition(1, 
            "Core (free)",
            new Guid("40e49a48-0bcd-429c-b6b1-a21e05886bdf"),
            "The core, free, open-source license covers 99% of all features in 2sxc. Most of the features are not even listed, as they are always enabled."
        ) { AutoEnable = true };

        public static readonly LicenseDefinition Patron = new LicenseDefinition(101, 
            "Patron/Supporter",
            new Guid("61d0bf11-187c-4ae8-9b76-a2c3d4beaad7"),
            "Patrons / supporters of 2sxc get some additional features as a thank you for supporting 2sxc."
        );

        public static readonly LicenseDefinition WebFarmCache = new LicenseDefinition(301, 
            "WebFarmCache",
            new Guid("ed5ca2e7-4c13-422c-ad8f-e47e78e3d0d9"),
            "DNN installations which run as a Farm can enable WebFarm caching to ensure the servers are in sync when something is edited."
        );

        public static readonly LicenseDefinition LightSpeed = new LicenseDefinition(201, 
            "Lightspeed",
            new Guid("4c4c7f24-649e-4ddc-b3cd-dd093552222d"),
            "BETA: LightSpeed is a special high-performance output cache which caches all output and automatically rebuilds when data it depends on is updated."
        );

        public static readonly LicenseDefinition EnterpriseCms = new LicenseDefinition(202, 
            "EnterpriseCms",
            new Guid("da7274c1-b893-4edb-8acb-ae2995a07321"),
            "BETA: Site Farms (name not final) is enables features to share app definitions across many sites. "
        );

        public static readonly LicenseDefinition CoreBeta = new LicenseDefinition(1000, "Beta",
            new Guid("a96277f0-df0e-4dc2-a9a6-4951fb43f26f"),
            "This enables beta features in 2sxc which are not yet final. "
        );

        private static IReadOnlyCollection<LicenseDefinition> CreateList()
        {
            return (new List<LicenseDefinition>
            {
                CoreFree,
                CoreBeta,
                Patron,
                WebFarmCache,
                LightSpeed,
                EnterpriseCms,
            }).AsReadOnly();
        }

        public static LicenseDefinition Find(string guid) => Licenses.FirstOrDefault(l => l.Guid.ToString().Equals(guid, StringComparison.InvariantCultureIgnoreCase));
    }
}
