﻿using System;
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