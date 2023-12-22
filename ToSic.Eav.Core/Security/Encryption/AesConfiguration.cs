using System;
using System.Text;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Security.Encryption;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AesConfiguration
{
    public AesConfiguration(bool useDefaults = false)
    {
        // Defaults for basic scenarios where we just encrypt so the stored data is not plain-text
        // But where the values themselves are not really secret
        if (useDefaults)
        {
            Salt = "aselrias38490a32";  // Random
            InitializationVector = "8947az34awl34kjq"; // Random
            Password = "this-is-a-trivial-password";
        }
    }

    /// <summary>
    /// Salt to use with the Password
    /// </summary>
    public string Salt { get; set; }
    public string Salt64 { get; set; }

    public byte[] SaltBytes() => string.IsNullOrWhiteSpace(Salt64)
        ? Encoding.UTF8.GetBytes(Salt)
        : Convert.FromBase64String(Salt64);


    /// <summary>
    /// recommended by https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.rfc2898derivebytes?view=net-7.0
    /// </summary>
    public int KeyGenIterations { get; set; } = 1000;

    public int KeySize { get; set; } = 256;

    public string InitializationVector { get; set; }
    public string InitializationVector64 { get; set; }
    public byte[] InitializationVectorBytes() => string.IsNullOrWhiteSpace(InitializationVector64)
        ? Encoding.UTF8.GetBytes(InitializationVector)
        : Convert.FromBase64String(InitializationVector64);

    public string Password { get; set; }

}