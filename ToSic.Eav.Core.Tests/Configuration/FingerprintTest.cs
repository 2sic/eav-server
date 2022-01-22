﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Run;
using ToSic.Eav.Security.Fingerprint;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Core.Tests.Configuration
{
    [TestClass]
    public class FingerprintTest: TestBaseDiPlumbing
    {
        [TestMethod]
        public void FingerprintExistsAndStaysTheSame()
        {
            var fingerprint = Build<SystemFingerprint>().GetFingerprint();
            Assert.IsNotNull(fingerprint);
            Assert.IsTrue(fingerprint.Length > 10);
            var fingerprint2 = Build<SystemFingerprint>().GetFingerprint();

            Assert.AreEqual(fingerprint, fingerprint2, "the fingerprint should stay the same");
        }
    }
}
