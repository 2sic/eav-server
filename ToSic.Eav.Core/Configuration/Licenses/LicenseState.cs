using System;

namespace ToSic.Eav.Configuration.Licenses
{
    public class LicenseState
    {
        public string LicenseKey { get; internal set; }

        public Guid EntityGuid { get; internal set; }

        public LicenseType License { get; internal set; }

        public bool Enabled => EnabledState && Valid;

        public bool EnabledState { get; internal set; }

        public bool Valid => ValidExpired && ValidSignature && ValidFingerprint && ValidVersion;

        public bool ValidExpired { get; internal set; }

        public bool ValidSignature { get; internal set; }

        public bool ValidFingerprint { get; internal set; }

        public bool ValidVersion { get; internal set; }
    }
}
