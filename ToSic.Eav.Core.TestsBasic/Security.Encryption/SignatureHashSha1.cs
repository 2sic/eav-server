using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ToSic.Eav.Core.Tests.Signature;

public class SignatureHashSha1
{

    [Fact]
    public void SignatureSha1ValidationTest()
    {
        var Json = TestData.Json;

        var encoding = new UnicodeEncoding();
        var data = encoding.GetBytes(Json);

        // get hash (SHA1 IS LESS SECURE)
        var sha1 = new SHA1Managed();
        var hash = sha1.ComputeHash(data);

        // sign hash
        var privateCert = new X509Certificate2(Convert.FromBase64String(TestKeys.SelfSigned2.Private), "", X509KeyStorageFlags.PersistKeySet);
        var csp = (RSACryptoServiceProvider)privateCert.PrivateKey;
        var signature = csp.SignHash(hash, "SHA1");
        var signatureBase64 = Convert.ToBase64String(signature);
        Trace.WriteLine($"server: signature Base64 {signatureBase64}");


        Trace.WriteLine("client:  Verify signature... SHA1 IS LESS SECURE");


        var encodingClient = new UnicodeEncoding();
        var dataClient = encodingClient.GetBytes(Json);
        var signatureClient = Convert.FromBase64String(signatureBase64);

        // get hash
        var sha1Client = new SHA1Managed();
        var hashClient = sha1Client.ComputeHash(dataClient);

        // verify the signature with the hash
        var publicCert = new X509Certificate2(Convert.FromBase64String(TestKeys.SelfSigned2.Public), "");
        var cspPublic = (RSACryptoServiceProvider)publicCert.PublicKey.Key;
        var isValidSignature = cspPublic.VerifyHash(hashClient, "SHA1", signatureClient);
        Trace.WriteLine($"client:  Signature valid {isValidSignature}");

        Assert.Equal(true, isValidSignature);
    }
}