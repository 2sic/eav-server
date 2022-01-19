using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Configuration.Licenses
{
    internal class LicenseCatalog
    {
        public static DateTime UnlimitedExpiry = new DateTime(2099, 12, 31);

        public static IReadOnlyCollection<LicenseDefinition> Licenses => _licenseTypes ?? (_licenseTypes = CreateList());
        private static IReadOnlyCollection<LicenseDefinition> _licenseTypes;

        public static readonly LicenseDefinition CoreFree = new LicenseDefinition(1, "Core (free)",new Guid("40e49a48-0bcd-429c-b6b1-a21e05886bdf")) { AutoEnable = true };

        public static readonly LicenseDefinition PatreonSupporter = new LicenseDefinition(101, "PatreonSupporter",new Guid("61d0bf11-187c-4ae8-9b76-a2c3d4beaad7"));

        public static readonly LicenseDefinition WebFarm = new LicenseDefinition(301, "WebFarmCache", new Guid("ed5ca2e7-4c13-422c-ad8f-e47e78e3d0d9"));

        public static readonly LicenseDefinition LightSpeed = new LicenseDefinition(201, "Lightspeed", new Guid("4c4c7f24-649e-4ddc-b3cd-dd093552222d"));

        public static readonly LicenseDefinition SitesFarm = new LicenseDefinition(202, "SitesFarm", new Guid("da7274c1-b893-4edb-8acb-ae2995a07321"));

        public static readonly LicenseDefinition CoreBeta = new LicenseDefinition(1000, "Beta", new Guid("a96277f0-df0e-4dc2-a9a6-4951fb43f26f"));

        private static IReadOnlyCollection<LicenseDefinition> CreateList()
        {
            return (new List<LicenseDefinition>
            {
                CoreFree,
                CoreBeta,
                PatreonSupporter,
                WebFarm,
                LightSpeed,
                SitesFarm,
            }).AsReadOnly();
        }

        public static LicenseDefinition Find(string guid) => Licenses.FirstOrDefault(l => l.Guid.ToString().Equals(guid, StringComparison.InvariantCultureIgnoreCase));
    }
}
