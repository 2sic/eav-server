using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Run;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Core.Tests.Configuration
{
    [TestClass]
    public class FingerprintTest
    {
        [TestMethod]
        public void FingerprintExistsAndStaysTheSame()
        {
            var fingerprint = EavTestBase.Resolve<IFingerprint>().GetSystemFingerprint(); // Eav.Configuration.Fingerprint.System;
            Assert.IsNotNull(fingerprint);
            Assert.IsTrue(fingerprint.Length > 10);
            var fingerprint2 = EavTestBase.Resolve<IFingerprint>().GetSystemFingerprint();

            Assert.AreEqual(fingerprint, fingerprint2, "the fingerprint should stay the same");
        }
    }
}
