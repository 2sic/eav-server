namespace ToSic.Eav.Run.Unknown
{
    public sealed class UnknownFingerprint: IFingerprint
    {
        public string GetSystemFingerprint() => "eav-server-fingerprint-unknown";
    }
}
