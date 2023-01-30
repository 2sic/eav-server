﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Security.Encryption
{
    /// <summary>
    /// AES Cryptography
    /// Important: Must NOT use AesManaged from .net, because it's not FIPS compliant
    /// </summary>
    public class AesCryptographyService: ServiceBase, ICanDebug
    {
        #region Empty Constructor / DI

        private readonly Rfc2898Generator _keyGen;

        public AesCryptographyService(Rfc2898Generator keyGen) : base("Eav.EncAes")
        {
            _keyGen = keyGen;
        }

        #endregion

        public EncryptionResult<string> EncryptToBase64(string value, AesConfiguration configuration = default)
            => Encrypt<AesCryptoServiceProvider>(value, configuration).ToBase64();

        private EncryptionResult<byte[]> Encrypt<T>(string value, AesConfiguration configuration = default
        ) where T : SymmetricAlgorithm, new() => Log.Func(enabled: Debug, func: l =>
        {
            configuration = configuration ?? new AesConfiguration(true);
            var saltBytes = configuration.SaltBytes();
            var valueBytes = Encoding.UTF8.GetBytes(value);

            using (var cipher = new T { Mode = CipherMode.CBC })
                try
                {
                    using (var encryption = cipher.CreateEncryptor(_keyGen.GetKeyBytes(configuration), cipher.IV))
                    using (var memoryStream = new MemoryStream())
                    using (var writer = new CryptoStream(memoryStream, encryption, CryptoStreamMode.Write))
                    {
                        writer.Write(valueBytes, 0, valueBytes.Length);
                        writer.FlushFinalBlock();
                        var encrypted = memoryStream.ToArray();

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
        

        public string DecryptFromBase64(string value, AesConfiguration configuration)
            => DecryptFromBase64<AesCryptoServiceProvider>(value, configuration);

        private string DecryptFromBase64<T>(string value64, AesConfiguration configuration)
            where T : SymmetricAlgorithm, new()
        => Log.Func(enabled: Debug, func: l =>
        {
            l.A(Debug, $"IV: {Log.Dump(configuration.InitializationVectorBytes())} from '{configuration.InitializationVector64 ?? configuration.InitializationVector}'");
            l.A(Debug, $"Salt: {Log.Dump(configuration.SaltBytes())} from '{configuration.Salt64 ?? configuration.Salt}'");
            return Decrypt<T>(Convert.FromBase64String(value64), configuration);
        });

        public string Decrypt<T>(byte[] value, AesConfiguration configuration
        ) where T : SymmetricAlgorithm, new() => Log.Func(enabled: Debug, func: l =>
        {
            using (var cipher = new T { Mode = CipherMode.CBC })
                try
                {
                    using (var decryptor = cipher.CreateDecryptor(_keyGen.GetKeyBytes(configuration), configuration.InitializationVectorBytes()))
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