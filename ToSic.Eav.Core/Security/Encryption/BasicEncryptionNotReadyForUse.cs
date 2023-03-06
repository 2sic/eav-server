using System;
using System.Security.Cryptography;
using System.Text;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Security.Encryption
{
    // IMPORTANT: this does not work ATM, and it's not used anywhere
    // Don't use at all please
    // it's just included so it could be finished some time soon

    // Inspired by
    // https://damienbod.com/2020/08/19/symmetric-and-asymmetric-encryption-in-net-core/

    [PrivateApi]
    public class BasicEncryptionNotReadyForUse
    {
        // Remember to update encrypted settings if these parameters change
        private const string DefaultInsecureIv = "salt";
        private const string DefaultInsecureEncryptionKey = "just-a-dummy-key-for-now-change-once-things-get-more-secure";

        private string Base64(string iv)
        {
            var bytes = Encoding.UTF8.GetBytes(iv);
            return Convert.ToBase64String(bytes);
        }

        // TODO: encrypt
        // note that we're using very simple symetric encryption ATM
        // and the use of this is limited to some simple keys in settings
        // Because of this, it's not critical yet, BUT it's important that
        // once more critical data is stored, it will use a more advanced encryption and probably asymmetric
        public string Encrypt(string data)
        {
            return Encrypt(data, Base64(DefaultInsecureIv), Base64(DefaultInsecureEncryptionKey));
        }

        public string Encrypt(string text, string IV, string key)
        {
            Aes cipher = CreateCipher(key);
            cipher.IV = Convert.FromBase64String(IV);

            ICryptoTransform cryptTransform = cipher.CreateEncryptor();
            byte[] plaintext = Encoding.UTF8.GetBytes(text);
            byte[] cipherText = cryptTransform.TransformFinalBlock(plaintext, 0, plaintext.Length);

            return Convert.ToBase64String(cipherText);
        }

        // todo: decrypt
        public string Decrypt(string data)
        {
            return Decrypt(data, DefaultInsecureIv, DefaultInsecureEncryptionKey);
        }

        public string Decrypt(string encryptedText, string IV, string key)
        {
            Aes cipher = CreateCipher(key);
            cipher.IV = Convert.FromBase64String(IV);

            ICryptoTransform cryptTransform = cipher.CreateDecryptor();
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] plainBytes = cryptTransform.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }


        //private string GetEncodedRandomString(int length)
        //{
        //    var base64 = Convert.ToBase64String(GenerateRandomBytes(length));
        //    return base64;
        //}

        private Aes CreateCipher(string keyBase64)
        {
            // Default values: Keysize 256, Padding PKC27
            Aes cipher = Aes.Create();
            cipher.Mode = CipherMode.CBC;  // Ensure the integrity of the ciphertext if using CBC

            cipher.Padding = PaddingMode.ISO10126;
            cipher.Key = Convert.FromBase64String(keyBase64);

            return cipher;
        }

        //private byte[] GenerateRandomBytes(int length)
        //{
        //    var byteArray = new byte[length];
        //    RandomNumberGenerator.Fill(byteArray);
        //    return byteArray;
        //}
    }
}
