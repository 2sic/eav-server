using System.Security.Cryptography;
using System.Text;

namespace ToSic.Sys.Security.Encryption;

internal class Hasher
{
    internal static string Hash(HashAlgorithm hasher, string value)
        => Hash(hasher, Encoding.UTF8.GetBytes(value));

    internal static string Hash(HashAlgorithm hasher, byte[] value)
    {
        var hash = new StringBuilder();
        var crypto = hasher.ComputeHash(value);
        foreach (var theByte in crypto)
            hash.Append(theByte.ToString("x2"));
        return hash.ToString();
    }

}