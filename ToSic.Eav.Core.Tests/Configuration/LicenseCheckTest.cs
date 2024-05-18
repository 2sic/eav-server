using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.Internal.Loaders;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Core.Tests.Configuration;

[TestClass]
public class LicenseCheckTest: TestBaseDiEavFullAndDb
{
    public LicenseCheckTest() => GetService<EavSystemLoader>().LoadLicenseAndFeatures();

    [TestMethod]
    public void StoreLicense()
    {
        var licenseService = GetService<ILicenseService>();
        var license = licenseService.All.FirstOrDefault(l => l.Title == "2sxc License Test License");

        Assert.IsNotNull(license);
        Assert.IsTrue(license.FingerprintIsValid);
        Assert.IsTrue(license.ExpirationIsValid);
        Assert.IsTrue(license.SignatureIsValid);
        Assert.IsTrue(license.VersionIsValid);
    }
}