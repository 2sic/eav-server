using ToSic.Eav.Documentation;

namespace ToSic.Eav.Run
{
    /// <summary>
    /// Defines all classes which can provide fingerprints - to delegate fingerprint-creation to the environment. 
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public interface IFingerprintProvider
    {
        string GetSystemFingerprint();
    }
}