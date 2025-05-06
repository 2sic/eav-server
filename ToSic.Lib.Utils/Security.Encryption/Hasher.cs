using System.Security.Cryptography;
using System.Text;

namespace ToSic.Eav.Security.Encryption;

internal class Hasher
{
    internal static string Hash(HashAlgorithm hasher, string value)
    {
        var hash = new StringBuilder();
        var crypto = hasher.ComputeHash(Encoding.UTF8.GetBytes(value));
        foreach (var theByte in crypto)
            hash.Append(theByte.ToString("x2"));
        return hash.ToString();
    }

}