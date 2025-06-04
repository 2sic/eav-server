using System.Security.Cryptography;
using System.Text;
using ToSic.Lib.Services;

namespace ToSic.Sys.Security.Encryption;

/// <summary>
/// AES Cryptography
/// Important: Must NOT use AesManaged from .net, because it's not FIPS compliant
/// </summary>
/// <remarks>
/// In v19.03.03 we changed `new AesCryptoServiceProvider()` to be `Aes.Create()` because of obsolete warnings
/// </remarks>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AesCryptographyService(Rfc2898Generator keyGen) : ServiceBase("Eav.EncAes"), ICanDebug
{
    public EncryptionResult<string> EncryptToBase64(string value, AesConfiguration? configuration = default)
        => Encrypt(value, configuration)
            .ToBase64();

    private EncryptionResult<byte[]> Encrypt(string value, AesConfiguration? configuration = default)
    {
        var l = Log.Fn<EncryptionResult<byte[]>>(enabled: Debug);
        configuration ??= new();
        var saltBytes = configuration.SaltBytes();
        var valueBytes = Encoding.UTF8.GetBytes(value);

        using var cipher = Aes.Create();
        cipher.Mode = CipherMode.CBC;
        try
        {
            var keyBytes = keyGen.GetKeyBytes(configuration);
            var iv = cipher.IV;
            using var encryption = cipher.CreateEncryptor(keyBytes, iv);
            using var memoryStream = new MemoryStream();
            using var writer = new CryptoStream(memoryStream, encryption, CryptoStreamMode.Write);
            writer.Write(valueBytes, 0, valueBytes.Length);
            writer.FlushFinalBlock();
            var encrypted = memoryStream.ToArray();

            var result = new EncryptionResult<byte[]>
            {
                Value = encrypted,
                Iv = iv,
                Salt = saltBytes
            };
            return l.ReturnAsOk(result);
        }
        finally
        {
            cipher.Clear();
        }
    }
        

    public string DecryptFromBase64(string value, AesConfiguration configuration)
    {
        var l = Log.Fn<string>(enabled: Debug);
        l.A(Debug, $"IV: {Log.Dump(configuration.InitializationVectorBytes())} from '{configuration.InitializationVector64 ?? configuration.InitializationVector}'");
        l.A(Debug, $"Salt: {Log.Dump(configuration.SaltBytes())} from '{configuration.Salt64 ?? configuration.Salt}'");
        return Decrypt(Convert.FromBase64String(value), configuration);
    }

    private string Decrypt(byte[] value, AesConfiguration configuration)
    {
        var l = Log.Fn<string>(enabled: Debug);
        using var cipher = Aes.Create();
        cipher.Mode = CipherMode.CBC;
        try
        {
            var keyBytes = keyGen.GetKeyBytes(configuration);
            var iv = configuration.InitializationVectorBytes();
            using var decryptor = cipher.CreateDecryptor(keyBytes, iv);
            using var from = new MemoryStream(value);
            using var reader = new CryptoStream(from, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(reader);
            var plaintext = srDecrypt.ReadToEnd();
            return l.ReturnAsOk(plaintext);
        }
        finally
        {
            cipher.Clear();
        }
    }

    public bool Debug { get; set; }
}