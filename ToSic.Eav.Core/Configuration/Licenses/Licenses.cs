using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ToSic.Eav.Configuration.Licenses
{
    public class Licenses
    {
        public static List<LicenseState> All { get; private set; } = new List<LicenseState>();

        public static IImmutableDictionary<Guid, LicenseState> Enabled { get; private set; } =
            new Dictionary<Guid, LicenseState>().ToImmutableDictionary();

        public static long CacheTimestamp;

        public static bool IsEnabled(Guid licenseId) => Enabled.ContainsKey(licenseId);

        internal static void Update(List<LicenseState> licenses)
        {
            All = licenses;
            Enabled = licenses.Where(l => l.Enabled).ToImmutableDictionary(l => l.License.Guid, l => l);
            CacheTimestamp = DateTime.Now.Ticks;
        }
    }
}
