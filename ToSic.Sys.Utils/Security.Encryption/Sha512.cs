using System.Security.Cryptography;

namespace ToSic.Sys.Security.Encryption;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class Sha512
{
    /// <remarks>
    /// * In v19.03.03 we changed `new SHA512CryptoServiceProvider()` to be `SHA512.Create()` because of obsolete warnings
    /// </remarks>
    public static string Hash(string value) 
        => Hasher.Hash(SHA512.Create(), value);
}