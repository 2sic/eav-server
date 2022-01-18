using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Configuration.Licenses
{
    public class Licenses
    {
        public static List<LicenseState> All { get; internal set; } = new List<LicenseState>();

        public static long CacheTimestamp;

        public static bool Enabled(Guid licenseId) => All.Any(l => l.EntityGuid == licenseId);
    }
}
