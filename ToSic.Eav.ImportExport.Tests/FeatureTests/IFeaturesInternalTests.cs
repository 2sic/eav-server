using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Configuration;

namespace ToSic.Eav.ImportExport.Tests.FeatureTests
{
    [TestClass]
    public class IFeaturesInternalTests: FeatureTestsBase
    {
        public IFeaturesInternalTests()
        {
            FeaturesInternal = Resolve<IFeaturesInternal>();
        }

        internal IFeaturesInternal FeaturesInternal;


        [TestMethod]
        public void EnsureIFeaturesAndIFeaturesInternalAreSameSingleton()
        {
            var featuresNonInternal = Resolve<IFeaturesService>();
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
            var x = FeaturesInternal.Enabled(FeatureIds.PasteImageClipboard);
            Assert.IsTrue(x, "this should be enabled and non-expired");
        }

        [TestMethod]
        public void InventedFeatureGuid()
        {
            var inventedGuid = new Guid("4f3d0021-1c8b-4286-a33b-3210ed3b2d9a");
            var x = FeaturesInternal.Enabled(inventedGuid);
            Assert.IsFalse(x, "this should be enabled and expired");
        }
    }
}
