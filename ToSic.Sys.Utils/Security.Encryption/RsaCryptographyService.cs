using System.Security.Cryptography;
using ToSic.Lib.Helpers;
using ToSic.Sys.Configuration;

namespace ToSic.Sys.Security.Encryption
{
    public class RsaCryptographyService(IGlobalConfiguration globalConfiguration)
    {
        // Set key size to 2048 bits
        private const int DwKeySize = 2048;

        // Generate PEM format for public key
        private const bool Pem = false;
        
        public byte[] Decrypt(string encryptedData)
        {
            using var rsa = RSA.Create(DwKeySize);

            rsa.FromXmlString(PrivateKey ?? throw new NullReferenceException($"{nameof(PrivateKey)} cannot be null."));

            // Decrypt
            var dataToDecrypt = Convert.FromBase64String(encryptedData);
            return rsa.Decrypt(dataToDecrypt, RSAEncryptionPadding.OaepSHA256);
        }

        public string? PublicKey 
            => _genKeysInLock.Call(
                conditionToGenerate: () => _publicKey == null,
                generator: () => GetOrCreateKeys().publicKey,
                cacheOrFallback: () => _publicKey
            ).Result;

        private string? PrivateKey
            => _genKeysInLock.Call(
                conditionToGenerate: () => _privateKey == null,
                generator: () => GetOrCreateKeys().privateKey,
                cacheOrFallback: () => _privateKey
            ).Result;

        private (string publicKey, string privateKey) GetOrCreateKeys()
        {
            if (_publicKey != null && _privateKey != null)
                return (_publicKey, _privateKey);

            // Generate keys if they don't exist
            if (!File.Exists(PublicKeyPath) || !File.Exists(PrivateKeyPath))
                GenKeys();

            // Load the RSA public and private key
            _publicKey = File.ReadAllText(PublicKeyPath);
            _privateKey = File.ReadAllText(PrivateKeyPath);

            return (_publicKey, _privateKey);
        }
        private string? _publicKey;
        private string? _privateKey;
        private readonly TryLockTryDo _genKeysInLock = new();
        
        private void GenKeys()
        {
            using var rsa = RSA.Create(DwKeySize);

            // create folder if not exist
            Directory.CreateDirectory(globalConfiguration.CryptoFolder());

            // save the RSA key pair to files in App_Data/2sxc.crypto folder
            ExportPublicKey(rsa);
            ExportPrivateKey(rsa);
        }
        private string PublicKeyPath => Path.Combine(globalConfiguration.CryptoFolder(), "public.key");
        private string PrivateKeyPath => Path.Combine(globalConfiguration.CryptoFolder(), "private.key");

        #region ExportKeys
        private void ExportPrivateKey(RSA rsa)
            => File.WriteAllText(PrivateKeyPath, rsa.ToXmlString(true));

        private void ExportPublicKey(RSA rsa)
        {
            var parameters = rsa.ExportParameters(false);
            var base64 = PublicKeyBase64(parameters);

            using var outputStream = File.CreateText(PublicKeyPath);
            if (Pem)
#pragma warning disable CS0162 // Unreachable code detected
                // ReSharper disable HeuristicUnreachableCode
            {
                outputStream.WriteLine("-----BEGIN PUBLIC KEY-----");
                for (var i = 0; i < base64.Length; i += 64)
                {
                    outputStream.WriteLine(base64, i, Math.Min(64, base64.Length - i));
                }
                outputStream.WriteLine("-----END PUBLIC KEY-----");
            }
            // ReSharper restore HeuristicUnreachableCode
#pragma warning restore CS0162 // Unreachable code detected
            else
            {
                outputStream.Write(base64);
            }
        }

        private static char[] PublicKeyBase64(RSAParameters parameters)
        {
            using var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write((byte)0x30); // SEQUENCE
            using var innerStream = new MemoryStream();
            var innerWriter = new BinaryWriter(innerStream);
            innerWriter.Write((byte)0x30); // SEQUENCE
            EncodeLength(innerWriter, 13);
            innerWriter.Write((byte)0x06); // OBJECT IDENTIFIER
            var rsaEncryptionOid = new byte[] { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01 };
            EncodeLength(innerWriter, rsaEncryptionOid.Length);
            innerWriter.Write(rsaEncryptionOid);
            innerWriter.Write((byte)0x05); // NULL
            EncodeLength(innerWriter, 0);
            innerWriter.Write((byte)0x03); // BIT STRING
            using var bitStringStream = new MemoryStream();
            var bitStringWriter = new BinaryWriter(bitStringStream);
            bitStringWriter.Write((byte)0x00); // # of unused bits
            bitStringWriter.Write((byte)0x30); // SEQUENCE
            using var paramsStream = new MemoryStream();
            var paramsWriter = new BinaryWriter(paramsStream);
            EncodeIntegerBigEndian(paramsWriter, parameters.Modulus!); // Modulus
            EncodeIntegerBigEndian(paramsWriter, parameters.Exponent!); // Exponent
            var paramsLength = (int)paramsStream.Length;
            EncodeLength(bitStringWriter, paramsLength);
            bitStringWriter.Write(paramsStream.GetBuffer(), 0, paramsLength);
            var bitStringLength = (int)bitStringStream.Length;
            EncodeLength(innerWriter, bitStringLength);
            innerWriter.Write(bitStringStream.GetBuffer(), 0, bitStringLength);
            var length = (int)innerStream.Length;
            EncodeLength(writer, length);
            writer.Write(innerStream.GetBuffer(), 0, length);

            return Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length).ToCharArray();
        }

        private static void EncodeLength(BinaryWriter stream, int length)
        {
            switch (length)
            {
                case < 0:
                    throw new ArgumentOutOfRangeException("length", "Length must be non-negative");
                case < 0x80:
                    // Short form
                    stream.Write((byte)length);
                    break;
                default:
                    {
                        // Long form
                        var temp = length;
                        var bytesRequired = 0;
                        while (temp > 0)
                        {
                            temp >>= 8;
                            bytesRequired++;
                        }
                        stream.Write((byte)(bytesRequired | 0x80));
                        for (var i = bytesRequired - 1; i >= 0; i--)
                        {
                            stream.Write((byte)(length >> (8 * i) & 0xff));
                        }

                        break;
                    }
            }
        }

        private static void EncodeIntegerBigEndian(BinaryWriter stream, byte[] value, bool forceUnsigned = true)
        {
            stream.Write((byte)0x02); // INTEGER
            var prefixZeros = 0;
            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] != 0) break;
                prefixZeros++;
            }
            if (value.Length - prefixZeros == 0)
            {
                EncodeLength(stream, 1);
                stream.Write((byte)0);
            }
            else
            {
                if (forceUnsigned && value[prefixZeros] > 0x7f)
                {
                    // Add a prefix zero to force unsigned if the MSB is 1
                    EncodeLength(stream, value.Length - prefixZeros + 1);
                    stream.Write((byte)0);
                }
                else
                {
                    EncodeLength(stream, value.Length - prefixZeros);
                }
                for (var i = prefixZeros; i < value.Length; i++)
                {
                    stream.Write(value[i]);
                }
            }
        }
        #endregion
    }
}
