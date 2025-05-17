using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Xunit.Abstractions;

namespace ToSic.Eav.Core.Tests.Signature;

public class SignatureHashSha1(ITestOutputHelper output)
{
    [Theory]
    [InlineData(TestData.Json)]
    public void SignatureSha1ValidationTest(string json)
    {
        var encoding = new UnicodeEncoding();
        var data = encoding.GetBytes(json);

        // get hash (SHA1 IS LESS SECURE)
        var hash = SHA1.Create().ComputeHash(data);

        // sign hash
        var privateCert = new X509Certificate2(Convert.FromBase64String(TestKeys.SelfSigned2.Private), "", X509KeyStorageFlags.PersistKeySet);

        var csp = privateCert.GetRSAPrivateKey();
        var signature = csp.SignHash(hash, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
        var signatureBase64 = Convert.ToBase64String(signature);
        output.WriteLine($"server: signature Base64 {signatureBase64}");


        output.WriteLine("client:  Verify signature... SHA1 IS LESS SECURE");


        var encodingClient = new UnicodeEncoding();
        var dataClient = encodingClient.GetBytes(json);
        var signatureClient = Convert.FromBase64String(signatureBase64);

        // get hash
        var hashClient = SHA1.Create().ComputeHash(dataClient);

        // verify the signature with the hash
        var publicCert = new X509Certificate2(Convert.FromBase64String(TestKeys.SelfSigned2.Public), "");

        var cspPublic = publicCert.GetRSAPublicKey();
        var isValidSignature = cspPublic.VerifyHash(hashClient, signatureClient, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
        output.WriteLine($"client:  Signature valid {isValidSignature}");

        True(isValidSignature);
    }
}