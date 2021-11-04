using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Run;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Core.Tests.Configuration
{
    [TestClass]
    public class FingerprintTest: TestBaseDiPlumbing
    {
        [TestMethod]
        public void FingerprintExistsAndStaysTheSame()
        {
            var fingerprint = Build<IFingerprint>().GetSystemFingerprint();
            Assert.IsNotNull(fingerprint);
            Assert.IsTrue(fingerprint.Length > 10);
            var fingerprint2 = Build<IFingerprint>().GetSystemFingerprint();

            Assert.AreEqual(fingerprint, fingerprint2, "the fingerprint should stay the same");
        }
    }
}
