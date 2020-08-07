namespace ToSic.Eav.Run.Basic
{
    public class BasicFingerprint: IFingerprintProvider
    {
        public string GetSystemFingerprint() => "eav-server-fingerprint-basic";
    }
}
