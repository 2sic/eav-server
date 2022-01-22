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
        public IImmutableDictionary<LicenseDefinition, LicenseState> Enabled => EnabledCache;

        /// <inheritdoc />
        public bool IsEnabled(LicenseDefinition license) => EnabledCache.ContainsKey(license);

        #endregion

        #region Internal stuff, caching, static

        private static List<LicenseState> AllCache { get; set; } = new List<LicenseState>();


        private static IImmutableDictionary<LicenseDefinition, LicenseState> EnabledCache { get; set; } =
            new Dictionary<LicenseDefinition, LicenseState>().ToImmutableDictionary();

        public static long CacheTimestamp;


        internal static void Update(List<LicenseState> licenses)
        {
            AllCache = licenses;
            EnabledCache = licenses.Where(l => l.Enabled).ToImmutableDictionary(l => l.License, l => l); ;
            CacheTimestamp = DateTime.Now.Ticks;
        }
        #endregion
    }
}
