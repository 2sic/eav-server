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
using System.Collections.Immutable;
using System.Linq;

namespace ToSic.Eav.Configuration.Licenses
{
    public class LicenseService : ILicenseService
    {
        #region Public APIs

        /// <inheritdoc />
        public List<LicenseState> All => AllCache;

        /// <inheritdoc />
        /// <remarks>
        /// We use the real static LicenseDefinition as an index, because this ensures that people can't inject other license objects to bypass security.
        /// </remarks>
        public IImmutableDictionary<Guid, LicenseState> Enabled => EnabledCache;

        /// <inheritdoc />
        public bool IsEnabled(LicenseDefinition license) => EnabledCache.ContainsKey(license.Guid);

        #endregion

        #region Internal stuff, caching, static

        private static List<LicenseState> AllCache { get; set; } = new List<LicenseState>();


        private static IImmutableDictionary<Guid, LicenseState> EnabledCache { get; set; } =
            new Dictionary<Guid, LicenseState>().ToImmutableDictionary();

        public static long CacheTimestamp;


        internal static void Update(List<LicenseState> licenses)
        {
            AllCache = licenses;
            EnabledCache = licenses
                .Where(l => l.Enabled)
                // must do Distinct = GroupBy+First to ensure we don't have duplicate keys
                .GroupBy(l => l.License)
                .Select(g => g.First())
                .ToImmutableDictionary(l => l.License.Guid, l => l); ;
            CacheTimestamp = DateTime.Now.Ticks;
            AllLicensesAreInvalid = AreAllLicensesInvalid();
        }

        public bool HaveValidLicense => !AllLicensesAreInvalid;
        
        internal static bool AllLicensesAreInvalid = false;

        internal static bool AreAllLicensesInvalid()
        {
            // if we do not have license for validation, than it can not be invalid
            if (AllCache.Count(l => l.License.AutoEnable != true) == 0) return false;
            
            // any valid license?
            foreach (var license in AllCache.Where(l => l.License.AutoEnable != true))
                if (license.Valid)
                    return false;
            return true;
        }
        #endregion
    }
}
