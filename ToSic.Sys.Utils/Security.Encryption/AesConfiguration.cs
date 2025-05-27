using System.Text;

namespace ToSic.Eav.Security.Encryption;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AesConfiguration
{
    /// <summary>
    /// Random _default_ salt to use with the Password, should probably never change, as things may already be encrypted with it.
    /// Other uses should then use a custom salt.
    /// </summary>
    private const string DefaultSalt = "aselrias38490a32";

    /// <summary>
    /// Random _default_ initialization vector to use with the Password, should probably never change, as things may already be encrypted with it.
    /// Other uses should then use a custom initialization vector.
    /// </summary>
    private const string DefaultInitializationVector = "8947az34awl34kjq"; // Random

    /// <summary>
    /// Random _default_ password to use with the Salt, should probably never change, as things may already be encrypted with it.
    /// Other uses should then use a custom password.
    /// </summary>
    private const string DefaultPassword = "this-is-a-trivial-password";

    /// <summary>
    /// Salt to use with the Password
    /// </summary>
    public string Salt { get; init; } = DefaultSalt;
    public string? Salt64 { get; init; }
    public string Password { get; init; } = DefaultPassword;


    /// <summary>
    /// recommended by https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.rfc2898derivebytes?view=net-7.0
    /// </summary>
    public int KeyGenIterations { get; set; } = 1000;

    public int KeySize { get; set; } = 256;

    public string InitializationVector { get; init; } = DefaultInitializationVector;

    public string? InitializationVector64 { get; init; }

    public byte[] SaltBytes() => string.IsNullOrWhiteSpace(Salt64)
        ? Encoding.UTF8.GetBytes(Salt)
        : Convert.FromBase64String(Salt64);

    public byte[] InitializationVectorBytes() => string.IsNullOrWhiteSpace(InitializationVector64)
        ? Encoding.UTF8.GetBytes(InitializationVector)
        : Convert.FromBase64String(InitializationVector64);


}