using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Security.Encryption
{
    // Note
    // Just copied this from https://stackoverflow.com/questions/273452/using-aes-encryption-in-c-sharp
    // Ok for temporary solution, must be improved long-term

    public class AesCryptographyService: ServiceBase, ICanDebug
    {
        #region Settings

        private static readonly int Iterations = 2;
        private static readonly int KeySize = 256;

        private static readonly string HastUsingSha256 = "SHA256";
        private static readonly string Salt = "aselrias38490a32"; // Random
        private static readonly string Vector = "8947az34awl34kjq"; // Random

        private const string BuiltInPasswordForSimpleUseOnly = "this-is-a-trivial-password";

        #endregion

        #region Empty Constructor

        public AesCryptographyService() : base("Eav.EncAes")
        {
        }

        #endregion

        // 2023-01-28 2dm disabled, because probably not FIPS compliant
        // https://github.com/2sic/2sxc/issues/2988
        //public static string EncryptAesManaged(string value, string password = BuiltInPasswordForSimpleUseOnly) 
        //    => Encrypt<AesManaged>(value, password);

        public EncryptionResult<string> EncryptToBase64(string value, string password = BuiltInPasswordForSimpleUseOnly) 
            => Encrypt<AesCryptoServiceProvider>(value, password, Salt).ToBase64();

        private EncryptionResult<byte[]> Encrypt<T>(string value, string password, string salt
        ) where T : SymmetricAlgorithm, new() => Log.Func(enabled: Debug, func: l =>
        {
            var saltBytes = Encoding.UTF8.GetBytes(salt);
            var valueBytes = Encoding.UTF8.GetBytes(value);

            using (var cipher = new T { Mode = CipherMode.CBC })
                try
                {
                    var keyBytes = GetKeyBytesFromPasswordAndSalt(password, saltBytes);

                    using (var encryption = cipher.CreateEncryptor(keyBytes, cipher.IV))
                    using (var memoryStream = new MemoryStream())
                    using (var writer = new CryptoStream(memoryStream, encryption, CryptoStreamMode.Write))
                    {
                        writer.Write(valueBytes, 0, valueBytes.Length);
                        writer.FlushFinalBlock();
                        var encrypted = memoryStream.ToArray();

                        l.A($"IV: {l.Dump(cipher.IV)}");

                        return new EncryptionResult<byte[]>
                        {
                            Value = encrypted,
                            Iv = cipher.IV,
                            Salt = saltBytes
                        };
                    }
                }
                finally
                {
                    cipher.Clear();
                }
        });

        private static byte[] GetKeyBytesFromPasswordAndSalt(string password, byte[] saltBytes)
        {
            var passwordBytes = new PasswordDeriveBytes(password, saltBytes, HastUsingSha256, Iterations);
            // GetBytes complains it's obsolete, but all alternatives seem to not work or not be for AES , ignore for now
            var keyBytes = passwordBytes.GetBytes(KeySize / 8);

            // WIP experimental not use GetBytes - doesn't seem to work for AES...
            //var pwBytes = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256);
            //var x = pwBytes.CryptDeriveKey()

            return keyBytes;
        }

        // 2023-01-28 2dm disabled, because probably not FIPS compliant
        // https://github.com/2sic/2sxc/issues/2988
        //public static string DecryptAesManaged(string value, string password = BuiltInPasswordForSimpleUseOnly) 
        //    => Decrypt<AesManaged>(value, password);

        public string DecryptFromBase64(string value, string password = BuiltInPasswordForSimpleUseOnly, string salt = default, string vector64 = default)
            => DecryptFromBase64<AesCryptoServiceProvider>(value, password, salt, vector64);

        public string DecryptFromBase64<T>(string value64, string password, string salt, string vector64)
            where T : SymmetricAlgorithm, new()
        => Log.Func(enabled: Debug, func: l =>
        {
            var valueBytes = Convert.FromBase64String(value64);
            var vectorBytes = vector64 == default
                ? Encoding.UTF8.GetBytes(Vector)
                : Convert.FromBase64String(vector64);
            var saltBytes = salt == default
                ? Encoding.UTF8.GetBytes(Salt)
                : Convert.FromBase64String(salt);
            l.A(Debug, $"IV: {Log.Dump(vectorBytes)} from '{vector64 ?? Vector}'");
            l.A(Debug, $"Salt: {Log.Dump(saltBytes)} from '{salt ?? Salt}'");
            return Decrypt<T>(valueBytes, password, saltBytes, vectorBytes);
        });

        public string Decrypt<T>(byte[] value, string password, byte[] salt, byte[] initializationVector
        ) where T : SymmetricAlgorithm, new() => Log.Func(enabled: Debug, func: l =>
        {
            using (var cipher = new T { Mode = CipherMode.CBC })
                try
                {
                    var keyBytes = GetKeyBytesFromPasswordAndSalt(password, salt);

                    using (var decryptor = cipher.CreateDecryptor(keyBytes, initializationVector))
                    using (var from = new MemoryStream(value))
                    using (var reader = new CryptoStream(from, decryptor, CryptoStreamMode.Read))
                    {
                        var decrypted = new byte[value.Length];
                        var decryptedByteCount = reader.Read(decrypted, 0, decrypted.Length);
                        return Encoding.UTF8.GetString(decrypted, 0, decryptedByteCount);
                    }
                }
                finally
                {
                    cipher.Clear();
                }
        });
        

        public bool Debug { get; set; }
    }

}
