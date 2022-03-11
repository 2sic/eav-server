using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Configuration;
using ToSic.Eav.Run;
using ToSic.Testing.Shared;
using ToSic.Testing.Shared.Platforms;

namespace ToSic.Eav.ImportExport.Tests.FeatureTests
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class IFeaturesTests: TestBaseDiEavFullAndDb
    {
        public IFeaturesTests() => Features = Build<IFeaturesService>();
        internal readonly IFeaturesService Features;

        protected override IServiceCollection SetupServices(IServiceCollection services = null)
        {
            return base.SetupServices(services).AddTransient<IPlatformInfo, TestPlatformWithLicense>();
        }


        [TestMethod]
        public void PasteClipboardActive()
        {
            var x = Features.Enabled(FeaturesCatalog.PasteImageFromClipboard.Guid);
            Assert.IsTrue(x, "this should be enabled and non-expired");
        }

        [TestMethod]
        public void InventedFeatureGuid()
        {
            var inventedGuid = new Guid("12345678-1c8b-4286-a33b-3210ed3b2d9a");
            var x = Features.Enabled(inventedGuid);
            Assert.IsFalse(x, "this should be enabled and expired");
        }
    }
}
