using ToSic.Eav.Documentation;

namespace ToSic.Eav.Configuration
{
    /// <summary>
    /// Defines all classes which can provide fingerprints - to delegate fingerprint-creation to the environment. 
    /// </summary>
    [PublicApi]
    public interface IFingerprintProvider
    {
        string GetSystemFingerprint();
    }
}