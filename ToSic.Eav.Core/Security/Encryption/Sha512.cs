using System.Security.Cryptography;

namespace ToSic.Eav.Security.Encryption;

internal class Sha512
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string Hash(string value) 
        => Sha256.Hash(new SHA512CryptoServiceProvider(), value);
}