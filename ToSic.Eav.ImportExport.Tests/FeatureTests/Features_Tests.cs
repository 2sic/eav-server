using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Configuration;

namespace ToSic.Eav.ImportExport.Tests.FeatureTests
{
    [TestClass]
    public class Features_Tests
    {
        [TestMethod]
        public void LoadFromConfiguration()
        {
            var x = Features.All;
            Assert.IsTrue(x.Count() > 2, "expect a few features in configuration");

        }

        [TestMethod]
        public void PasteClipboardActive()
        {
            var x = Features.Enabled(new Guid("f6b8d6da-4744-453b-9543-0de499aa2352"));
            Assert.IsTrue(x, "this shoudl be enabled and non-expired");
        }

        [TestMethod]
        public void HideLoveExpired()
        {
            var x = Features.Enabled(new Guid("4f3d0021-1c8b-4286-a33b-3210ed3b2d9a"));
            Assert.IsFalse(x, "this shoudl be enabled and expired");
        }
    }
}
