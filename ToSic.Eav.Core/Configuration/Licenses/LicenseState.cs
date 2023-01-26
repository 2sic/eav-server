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
    public class LicenseState
    {
        public LicenseState() { }

        public string Title { get; internal set; }
        public string LicenseKey { get; internal set; }

        public Guid EntityGuid { get; internal set; }

        public LicenseDefinition License { get; internal set; }

        public bool Enabled => EnabledState && Valid;

        /// <summary>
        /// The state as toggled in the settings - ATM always true, as we don't read the settings
        /// </summary>
        public bool EnabledState { get; internal set; } = true;

        public bool Valid => ValidExpired && ValidSignature && ValidFingerprint && ValidVersion;

        public DateTime Expiration { get; internal set; }

        public bool ValidExpired { get; internal set; }

        public bool ValidSignature { get; internal set; }

        public bool ValidFingerprint { get; internal set; }

        public bool ValidVersion { get; internal set; }
        
        public string Owner { get; internal set; }
    }
}
