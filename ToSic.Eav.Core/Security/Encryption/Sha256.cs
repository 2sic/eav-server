using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Security.Encryption;

[PrivateApi]
public class Sha256
{
    /// <summary>
    /// Compute a safe hash which hides what's inside, but is unique for each system.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <remarks>
    /// In v15.01 we changed this to use the CryptoServiceProvider which should be FIPS compliant
    /// </remarks>
    public static string Hash(string value)
    {
        var hash = new StringBuilder();
        using (var crypt = new SHA256CryptoServiceProvider())
        {
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(value));
            foreach (var theByte in crypto)
                hash.Append(theByte.ToString("x2"));
        }
        return hash.ToString();
    }

    public string SignBase64(string certificateBase64, byte[] data)
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


    public bool VerifyBase64(string publicCertBase64, string signatureBase64, byte[] dataClient)
    {
        var publicCert = new X509Certificate2(Convert.FromBase64String(publicCertBase64), "");
        var rsaParam = publicCert.GetRSAPublicKey().ExportParameters(false);
        var cspPublic = new RSACryptoServiceProvider();
        cspPublic.ImportParameters(rsaParam);

        var signatureClient = Convert.FromBase64String(signatureBase64);

        // verify the signature 
        var isValidSignature = cspPublic.VerifyData(dataClient, CryptoConfig.MapNameToOID("SHA256"), signatureClient);
        return isValidSignature;
    }

}