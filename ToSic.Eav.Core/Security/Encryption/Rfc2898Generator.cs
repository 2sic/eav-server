using System.Security.Cryptography;

namespace ToSic.Eav.Security.Encryption;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class Rfc2898Generator
{
    public Rfc2898Generator() { }

    public virtual byte[] GetKeyBytes(AesConfiguration config)
    {
        var saltBytes = config.SaltBytes();

#if NETFRAMEWORK
        // Note: as of .net standard 2.0 we can't specify the HashAlgorithm
        // This is why this is only implemented for .net Framework
        // For Oqtane, it must be implemented there
        // #cleanUpForNetStandard2.1
        var pwBytes = new Rfc2898DeriveBytes(config.Password, saltBytes, config.KeyGenIterations, HashAlgorithmName.SHA256);
        var keyNew = pwBytes.GetBytes(config.KeySize / 8);
        return keyNew;
#else
            throw new System.NotImplementedException();
#endif
    }
}