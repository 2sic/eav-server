using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Configuration;

namespace ToSic.Eav.ImportExport.Tests.FeatureTests
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class IFeaturesTests: FeatureTestsBase
    {
        public IFeaturesTests()
        {
            Features = Resolve<IFeaturesService>();
        }

        internal readonly IFeaturesService Features;


        [TestMethod]
        public void PasteClipboardActive()
        {
            var x = Features.Enabled(FeatureIds.PasteImageClipboard);
            Assert.IsTrue(x, "this should be enabled and non-expired");
        }

        [TestMethod]
        public void InventedFeatureGuid()
        {
            var inventedGuid = new Guid("4f3d0021-1c8b-4286-a33b-3210ed3b2d9a");
            var x = Features.Enabled(inventedGuid);
            Assert.IsFalse(x, "this should be enabled and expired");
        }
    }
}
