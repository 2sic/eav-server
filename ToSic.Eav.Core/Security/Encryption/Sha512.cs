using System.Security.Cryptography;

namespace ToSic.Eav.Security.Encryption;

internal class Sha512
{
    /// <remarks>
    /// * In v19.03.03 we changed `new SHA512CryptoServiceProvider()` to be `SHA512.Create()` because of obsolete warnings
    /// </remarks>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string Hash(string value) 
        => Hasher.Hash(SHA512.Create(), value);
}