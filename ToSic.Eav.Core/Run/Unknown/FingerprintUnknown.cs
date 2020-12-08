namespace ToSic.Eav.Run.Unknown
{
    public sealed class FingerprintUnknown: IFingerprint, IIsUnknown
    {
        public string GetSystemFingerprint() => "eav-server-fingerprint-unknown";
    }
}
