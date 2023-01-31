using System.Diagnostics;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Security.Encryption;

namespace ToSic.Eav.Core.Tests.Signature
{
    [TestClass]
    public class SignatureDataSha256
    {
        [DataRow("Test", "7ef0f36f907db03f760b71b7307785da6d40331a2844b34b1f23e573c13dbe16", "This is a test message to Hash")]
        [TestMethod]
        public void BasicHashing(string testName, string expected, string message)
        {
            var testNew = Sha256.Hash(message);
            Trace.WriteLine($"Hash: '{testNew}'");
            Assert.AreEqual(expected, testNew);
        }

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
