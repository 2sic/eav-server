using System.Security.Cryptography;
using System.Text;

namespace ToSic.Eav.Security.Encryption
{
    public class AesHybridCryptographyService(RsaCryptographyService rsa)
    {
        public string Decrypt(EncryptedData encryptedData)
        {
            // Decrypt the AES key
            var aesKeyBytes = rsa.Decrypt(encryptedData.Key);

            // Convert base64 strings to byte arrays
            var ivBytes = Convert.FromBase64String(encryptedData.Iv);

            // Decrypt the data using AES key
            // TODO: can we replace this method with existing AesCryptographyService
            using AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.Key = aesKeyBytes;
            aes.IV = ivBytes;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var encryptedDataBytes = Convert.FromBase64String(encryptedData.Data);

            // Get decrypted data
            using var decryptor = aes.CreateDecryptor();
            var decryptedDataBytes = decryptor.TransformFinalBlock(encryptedDataBytes, 0, encryptedDataBytes.Length);
            return Encoding.UTF8.GetString(decryptedDataBytes);
        }
    }
}
