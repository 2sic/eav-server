using ToSic.Eav.Logging;

namespace ToSic.Eav.Run.Unknown
{
    public sealed class FingerprintUnknown: IFingerprint, IIsUnknown
    {
        public FingerprintUnknown(WarnUseOfUnknown<FingerprintUnknown> warn)
        {

        }

        public string GetSystemFingerprint() => "eav-server-fingerprint-unknown";
    }
}
