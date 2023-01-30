using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using ToSic.Eav.Security.Encryption;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Core.Tests.Signature
{
    [TestClass]
    public class EncryptTests
    {
        private const string TestMessage = "This is a test message";
        private const string DummyPassword = "dummy-password";
        private const string PreviousEncryptionSha256 = "L3pBTTJ9+Ow1aRRWX4Flykh3UEfO7/XffcEuJPVKABg=";
        [TestMethod]
        public void TestBasicAesCrypto()
        {
            var crypto = new AesCryptographyService
            {
                Debug = true
            };
            var encrypted = crypto.EncryptToBase64(TestMessage, DummyPassword);
            Trace.WriteLine($"encrypted:'{encrypted.Value}'; IV:'{encrypted.Iv}'");
            Trace.WriteLine(crypto.Log.Dump());
            Assert.AreNotEqual(TestMessage, encrypted.Value);
            Assert.AreNotEqual(PreviousEncryptionSha256, encrypted.Value, "each encryption should give a different result - but that's not implemented yet");

            // reset crypto to get a fresh log
            crypto = new AesCryptographyService
            {
                Debug = true
            };
            var decrypted = crypto.DecryptFromBase64(encrypted.Value, DummyPassword, vector64: encrypted.Iv);
            Trace.WriteLine($"decrypted:{decrypted}");
            Trace.WriteLine(crypto.Log.Dump());
            Assert.AreEqual(TestMessage, decrypted);
        }

        [TestMethod]
        [Ignore("doesn't work at all yet, don't use")]
        public void EncryptAndDecriptUsingKeyPair()
        {

            var encryptor = new BasicEncryptionNotReadyForUse();
            var encrypted = encryptor.Encrypt(TestMessage);
            Assert.AreNotEqual(TestMessage, encrypted);

            var decrypted = encryptor.Decrypt(encrypted);
            Assert.AreEqual(TestMessage, decrypted);
        }
    }
}
