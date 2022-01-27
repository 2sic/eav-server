﻿/*
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
using System.Text.RegularExpressions;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Configuration.Licenses
{
    public class LicenseStored
    {
        /// <summary>
        /// Just a title to better label the license configuration
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Optional comments about the license
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// The customer license key which activated this license
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// A unique identifier for this configuration - serves as salt in the license
        /// </summary>
        public Guid GuidSalt { get; set; }

        /// <summary>
        /// Fingerprints of systems where this license applies
        /// </summary>
        public List<LicenseStoredDetails> Fingerprints { get; set; }

        /// <summary>
        /// Fingerprints of systems where this license applies
        /// </summary>
        public List<LicenseStoredDetails> Licenses { get; set; }

        /// <summary>
        /// List of 2sxc/eav versions which this license applies to
        /// Important for 
        /// </summary>
        public string Versions { get; set; }

        /// <summary>
        /// When the license becomes invalid
        /// </summary>
        public DateTime Expires { get; set; }

        /// <summary>
        /// Authority which generated this license
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// Date-Time generated
        /// </summary>
        public DateTime Generated { get; set; }

        /// <summary>
        /// Digital signature signing the certificate
        /// </summary>
        public string Signature { get; set; }

        public string[] LicensesArray => Licenses?.Select(l => l.Id).ToArray().TrimmedAndWithoutEmpty() ?? Array.Empty<string>();

        public string[] FingerprintsArray => Fingerprints?.Select(fp => fp.Id).ToArray().TrimmedAndWithoutEmpty() ?? Array.Empty<string>();

        public string GenerateIdentity()
        {
            var parts = new[]
            {
                "key: " + Key,
                "licenses:" + string.Join(",", LicensesArray), // 2. Add all licenses
                "fingerprints:" + string.Join(",", FingerprintsArray), // 3. Add all fingerprints
                "versions:" + Versions,
                "expires:" + Expires.ToString("yyyy-MM-dd"),
                "generated:" + Generated.ToString("yyyy-MM-dd"),
                "salt:" + GuidSalt
            };

            var licenseString = string.Join(";", parts);
            var licNoSpaces = Regex.Replace(licenseString, @"\s+", "");
            return licNoSpaces;
        }
    }

    public class LicenseStoredDetails
    {
        /// <summary>
        /// A fingerprint / License
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Optional comments, like what system it's for
        /// </summary>
        public string Comments { get; set; }
    }
}