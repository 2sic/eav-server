using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Configuration;
using ToSic.Eav.Run;
using ToSic.Testing.Shared;
using ToSic.Testing.Shared.Platforms;

namespace ToSic.Eav.ImportExport.Tests.FeatureTests
{
    [TestClass]
    public class IFeaturesInternalTests: TestBaseDiEavFullAndDb
    {
        public IFeaturesInternalTests()
        {
            FeaturesInternal = GetService<IFeaturesInternal>();
        }
        internal IFeaturesInternal FeaturesInternal;

        protected override void SetupServices(IServiceCollection services)
        {
            base.SetupServices(services);
            services.AddTransient<IPlatformInfo, TestPlatformPatronPerfectionist>();
        }


        [TestMethod]
        public void EnsureIFeaturesAndIFeaturesInternalAreSameSingleton()
        {
            var featuresNonInternal = GetService<IFeaturesService>();
            Assert.AreEqual(FeaturesInternal, featuresNonInternal, "They must be the identical object");
        }

        [TestMethod]
        public void LoadFromConfiguration()
        {
            var x = FeaturesInternal.All;
            Assert.IsTrue(x.Count() > 2, "expect a few features in configuration");

        }

        [TestMethod]
        public void PasteClipboardActive()
        {
            var x = FeaturesInternal.IsEnabled(BuiltInFeatures.PasteImageFromClipboard.Guid);
            Assert.IsTrue(x, "this should be enabled and non-expired");
        }

        [TestMethod]
        public void InventedFeatureGuid()
        {
            var inventedGuid = new Guid("12345678-1c8b-4286-a33b-3210ed3b2d9a");
            var x = FeaturesInternal.IsEnabled(inventedGuid);
            Assert.IsFalse(x, "this should be enabled and expired");
        }
    }
}
