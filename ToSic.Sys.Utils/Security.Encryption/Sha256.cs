using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace ToSic.Sys.Security.Encryption;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
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
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string Hash(string value) 
        => Hasher.Hash(SHA256.Create(), value);


    [ShowApiWhenReleased(ShowApiMode.Never)]
    public string SignBase64(string certificateBase64, byte[] data)
    {
        if (certificateBase64 == null) throw new ArgumentNullException(nameof(certificateBase64));
        if (data == null) throw new ArgumentNullException(nameof(data));

#if NETFRAMEWORK
        var certificate = new X509Certificate2(Convert.FromBase64String(certificateBase64), "", X509KeyStorageFlags.Exportable);
#else
        var certificate = X509CertificateLoader.LoadPkcs12(Convert.FromBase64String(certificateBase64), "", X509KeyStorageFlags.Exportable);
#endif
        var rsa = certificate.GetRSAPrivateKey()!;
        var signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var signatureBase64 = Convert.ToBase64String(signature);
        return signatureBase64;
    }    
    
    public bool VerifyBase64(string publicCertBase64, string signatureBase64, byte[] dataClient)
    {
        if (publicCertBase64 == null) throw new ArgumentNullException(nameof(publicCertBase64));
        if (signatureBase64 == null) throw new ArgumentNullException(nameof(signatureBase64));
        if (dataClient == null) throw new ArgumentNullException(nameof(dataClient));

#if NETFRAMEWORK
        var publicCert = new X509Certificate2(Convert.FromBase64String(publicCertBase64));
#else
        var publicCert = X509CertificateLoader.LoadCertificate(Convert.FromBase64String(publicCertBase64));
#endif
        var rsa = publicCert.GetRSAPublicKey()!;

        var signatureClient = Convert.FromBase64String(signatureBase64);

        // verify the signature 
        var isValidSignature = rsa.VerifyData(dataClient, signatureClient, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return isValidSignature;
    }

}