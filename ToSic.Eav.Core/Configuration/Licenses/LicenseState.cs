using System;

namespace ToSic.Eav.Configuration.Licenses
{
    public class LicenseState
    {
        public string Title { get; internal set; }
        public string LicenseKey { get; internal set; }

        public Guid EntityGuid { get; internal set; }

        public LicenseType License { get; internal set; }

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
    }
}
