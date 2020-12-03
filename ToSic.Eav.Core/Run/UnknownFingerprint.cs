namespace ToSic.Eav.Run.Basic
{
    public class UnknownFingerprint: IFingerprint
    {
        public sealed string GetSystemFingerprint() => "eav-server-fingerprint-unknown";
    }
}
