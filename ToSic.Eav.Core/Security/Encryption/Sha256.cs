using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ToSic.Eav.Security.Encryption;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class Sha256
{
    /// <summary>
    /// Compute a safe hash which hides what's inside, but is unique for each system.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <remarks>
    /// * In v15.01 we changed this to use the CryptoServiceProvider which is FIPS compliant
    /// * In v19.03.03 we changed `new SHA256CryptoServiceProvider()` to be `SHA256.Create()` because of obsolete warnings
    /// </remarks>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string Hash(string value) 
        => Hasher.Hash(SHA256.Create(), value);


    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
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