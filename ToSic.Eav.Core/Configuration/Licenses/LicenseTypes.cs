using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Configuration.Licenses
{
    internal class LicenseTypes
    {

        public static List<LicenseType> Licenses => _licenseTypes ?? (_licenseTypes = CreateList());
        private static List<LicenseType> _licenseTypes;

        public static LicenseType CoreFree = new LicenseType("Free",new Guid("40e49a48-0bcd-429c-b6b1-a21e05886bdf"));
        public static LicenseType CoreBeta = new LicenseType("Beta",new Guid("a96277f0-df0e-4dc2-a9a6-4951fb43f26f"));

        public static LicenseType Patreon = new LicenseType("Patreon",new Guid("61d0bf11-187c-4ae8-9b76-a2c3d4beaad7"));

        public static LicenseType WebFarm = new LicenseType("WebFarmCache", new Guid("ed5ca2e7-4c13-422c-ad8f-e47e78e3d0d9"));

        public static LicenseType LightSpeed = new LicenseType("Lightspeed", new Guid("4c4c7f24-649e-4ddc-b3cd-dd093552222d"));
        public static LicenseType SitesFarm = new LicenseType("SitesFarm", new Guid("da7274c1-b893-4edb-8acb-ae2995a07321"));

        private static List<LicenseType> CreateList()
        {
            return new List<LicenseType>()
            {
                CoreFree,
                CoreBeta,
                Patreon,
                WebFarm,
                LightSpeed,
                SitesFarm,
            };
        }

        public static LicenseType Find(string guid) => Licenses.FirstOrDefault(l => l.Guid.ToString().Equals(guid, StringComparison.InvariantCultureIgnoreCase));
    }
}
