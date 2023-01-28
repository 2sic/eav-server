using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ToSic.Eav.Security.Encryption
{
    // Note
    // Just copied this from https://stackoverflow.com/questions/273452/using-aes-encryption-in-c-sharp
    // Ok for temporary solution, must be improved long-term

    public static class BasicAesCryptography
    {
        #region Settings

        private static int _iterations = 2;
        private static int _keySize = 256;

        private static string _hash = "SHA256";
        private static string _salt = "aselrias38490a32"; // Random
        private static string _vector = "8947az34awl34kjq"; // Random

        private const string BuiltInPasswordForSimpleUseOnly = "this-is-a-trivial-password";

        #endregion

        // 2023-01-28 2dm disabled, because probably not FIPS compliant
        // https://github.com/2sic/2sxc/issues/2988
        //public static string EncryptAesManaged(string value, string password = BuiltInPasswordForSimpleUseOnly) 
        //    => Encrypt<AesManaged>(value, password);

        public static string EncryptAesCrypto(string value, string password = BuiltInPasswordForSimpleUseOnly) 
            => Encrypt<AesCryptoServiceProvider>(value, password);


        public static string Encrypt<T>(string value, string password) where T : SymmetricAlgorithm, new()
        {
            var vectorBytes = Encoding.ASCII.GetBytes(_vector);
            var saltBytes = Encoding.ASCII.GetBytes(_salt);
            var valueBytes = Encoding.UTF8.GetBytes(value);

            byte[] encrypted;
            using (var cipher = new T())
            {
                var passwordBytes = new PasswordDeriveBytes(password, saltBytes, _hash, _iterations);
                var keyBytes = passwordBytes.GetBytes(_keySize / 8);

                cipher.Mode = CipherMode.CBC;

                // test 2dm
                // doesn't work ATM, because when decrypting the IV needs to be knows
                //cipher.GenerateIV();
                //Trace.WriteLine("IV:" + Convert.ToBase64String(cipher.IV));

                using (var encryption = cipher.CreateEncryptor(keyBytes, vectorBytes))
                using (var to = new MemoryStream())
                using (var writer = new CryptoStream(to, encryption, CryptoStreamMode.Write))
                {
                    writer.Write(valueBytes, 0, valueBytes.Length);
                    writer.FlushFinalBlock();
                    encrypted = to.ToArray();
                }

                cipher.Clear();
            }
            return Convert.ToBase64String(encrypted);
        }

        // 2023-01-28 2dm disabled, because probably not FIPS compliant
        // https://github.com/2sic/2sxc/issues/2988
        //public static string DecryptAesManaged(string value, string password = BuiltInPasswordForSimpleUseOnly) 
        //    => Decrypt<AesManaged>(value, password);

        public static string DecryptAesCrypto(string value, string password = BuiltInPasswordForSimpleUseOnly) 
            => Decrypt<AesCryptoServiceProvider>(value, password);

        public static string Decrypt<T>(string value, string password) where T : SymmetricAlgorithm, new()
        {
            var vectorBytes = Encoding.ASCII.GetBytes(_vector);
            var saltBytes = Encoding.ASCII.GetBytes(_salt);
            var valueBytes = Convert.FromBase64String(value);

            byte[] decrypted;
            int decryptedByteCount;

            using (var cipher = new T())
            {
                var _passwordBytes = new PasswordDeriveBytes(password, saltBytes, _hash, _iterations);
                var keyBytes = _passwordBytes.GetBytes(_keySize / 8);

                cipher.Mode = CipherMode.CBC;

                try
                {
                    using (var decryptor = cipher.CreateDecryptor(keyBytes, vectorBytes))
                    using (var from = new MemoryStream(valueBytes))
                    using (var reader = new CryptoStream(from, decryptor, CryptoStreamMode.Read))
                    {
                        decrypted = new byte[valueBytes.Length];
                        decryptedByteCount = reader.Read(decrypted, 0, decrypted.Length);
                    }
                }
                catch
                {
                    return null;
                }

                cipher.Clear();
            }
            return Encoding.UTF8.GetString(decrypted, 0, decryptedByteCount);
        }

    }

}
