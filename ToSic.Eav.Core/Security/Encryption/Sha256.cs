using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace ToSic.Eav.Security.Encryption
{
    public class Sha256
    {
        public static string SignBase64(string certificateBase64, byte[] data)
        {
            var certificate = new X509Certificate2(Convert.FromBase64String(certificateBase64),
                "", X509KeyStorageFlags.Exportable);
            var exportedKeyMaterial = certificate.PrivateKey.ToXmlString(true);
            var csp = new RSACryptoServiceProvider(new CspParameters(24 /* PROV_RSA_AES */)) { PersistKeyInCsp = false };
            csp.FromXmlString(exportedKeyMaterial);
            var signature = csp.SignData(data, CryptoConfig.MapNameToOID("SHA256"));
            var signatureBase64 = Convert.ToBase64String(signature);
            return signatureBase64;
        }


        public static bool VerifyBase64(string publicCertBase64, string signatureBase64,
            byte[] dataClient)
        {
            var publicCert = new X509Certificate2(Convert.FromBase64String(publicCertBase64), "");
            var cspPublic = (RSACryptoServiceProvider)publicCert.PublicKey.Key;
            var signatureClient = Convert.FromBase64String(signatureBase64);

            // verify the signature 
            var isValidSignature = cspPublic.VerifyData(dataClient, CryptoConfig.MapNameToOID("SHA256"), signatureClient);
            return isValidSignature;
        }
    }
}
