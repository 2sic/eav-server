using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Core.Tests.Configuration
{
    [TestClass]
    public class LicenseCheckTest: TestBaseDiEavFullAndDb
    {
        public LicenseCheckTest() => Build<EavSystemLoader>().LoadLicenseAndFeatures();

        [TestMethod]
        public void StoreLicense()
        {
            var licenseService = Build<ILicenseService>();
            var license = licenseService.All.FirstOrDefault(l => l.Title == "2sxc License Test License");

            Assert.IsNotNull(license);
            Assert.IsTrue(license.FingerprintIsValid);
            Assert.IsTrue(license.ExpirationIsValid);
            Assert.IsTrue(license.SignatureIsValid);
            Assert.IsTrue(license.VersionIsValid);
        }
    }
}
