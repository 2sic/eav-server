using System.Security.Cryptography;

namespace ToSic.Sys.Security.Encryption;

public class Sha512
{
    /// <remarks>
    /// * In v19.03.03 we changed `new SHA512CryptoServiceProvider()` to be `SHA512.Create()` because of obsolete warnings
    /// </remarks>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string Hash(string value) 
        => Hasher.Hash(SHA512.Create(), value);
}