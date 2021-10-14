using System.Diagnostics;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Security.Encryption;

namespace ToSic.Eav.Core.Tests.Signature
{
    [TestClass]
    public class SignatureDataSha256
    {
        [TestMethod]
        public void SignatureSha256ValidationTest()
        {
            var Json = TestData.Json;
            // *** server
            var encoding = new UnicodeEncoding();
            var data = encoding.GetBytes(Json);

            var signatureBase64 = new Sha256().SignBase64(TestKeys.SelfSigned2048Sha256.Private, data);
            Trace.WriteLine($"server: signature Base64 {signatureBase64}");
            Trace.WriteLine("client:  Verify signature...");

            // *** client
            var encodingClient = new UnicodeEncoding();
            var dataClient = encodingClient.GetBytes(Json);

            var isValidSignature = new Sha256().VerifyBase64(TestKeys.SelfSigned2048Sha256.Public, signatureBase64, dataClient);
            Trace.WriteLine($"client:  Signature valid {isValidSignature}");

            Assert.AreEqual(true, isValidSignature);
        }
    }
}
