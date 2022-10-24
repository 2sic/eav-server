using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using ToSic.Eav.Security.Encryption;

namespace ToSic.Eav.Core.Tests.Signature
{
    [TestClass]
    public class EncryptTests
    {
        private const string TestMessage = "This is a test message";
        private const string DummyPassword = "dummy-password";
        private const string PreviousEncryptionSha1 = "wuEeVGHucRfZHaAohx8dW5ZUEaK6jUNewJWeahGgapA=";
        private const string PreviousEncryptionSha256 = "L3pBTTJ9+Ow1aRRWX4Flykh3UEfO7/XffcEuJPVKABg=";
        [TestMethod]
        public void TestBasicAesCrypto()
        {
            var encrypted = BasicAesCryptography.Encrypt(TestMessage, DummyPassword);
            Trace.WriteLine($"encrypted:{encrypted}");

            Assert.AreNotEqual(TestMessage, encrypted);
            Assert.AreEqual(PreviousEncryptionSha256, encrypted, "each encryption should give a different result - but that's not implemented yet");
            var decrypted = BasicAesCryptography.Decrypt(encrypted, DummyPassword);
            Trace.WriteLine($"decrypted:{decrypted}");
            Assert.AreEqual(TestMessage, decrypted);

        }

        [TestMethod]
        [Ignore("doesn't work at all yet, don't use")]
        public void EncryptAndDecriptUsingKeyPair()
        {

            var encryptor = new BasicEncryption();
            var encrypted = encryptor.Encrypt(TestMessage);
            Assert.AreNotEqual(TestMessage, encrypted);

            var decrypted = encryptor.Decrypt(encrypted);
            Assert.AreEqual(TestMessage, decrypted);
        }
    }
}
