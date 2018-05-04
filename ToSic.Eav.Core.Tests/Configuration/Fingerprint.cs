using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.Core.Tests.Configuration
{
    [TestClass]
    public class Fingerprint
    {
        [TestMethod]
        public void FingerprintExistsAndStaysTheSame()
        {
            var fingerprint = Eav.Configuration.Fingerprint.System;
            Assert.IsNotNull(fingerprint);
            Assert.IsTrue(fingerprint.Length > 10);
            var fingerprint2 = Eav.Configuration.Fingerprint.System;

            Assert.AreEqual(fingerprint, fingerprint2, "the fingerprint should stay the same");
        }
    }
}
