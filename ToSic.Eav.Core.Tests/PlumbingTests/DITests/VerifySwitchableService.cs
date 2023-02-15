using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Lib.DI;

namespace ToSic.Eav.Core.Tests.PlumbingTests.DITests
{
    [TestClass]
    public class VerifySwitchableService: VerifySwitchableServiceBase
    {

        [TestMethod]
        public void FindKeepService()
        {
            var switcher = GetService<ServiceSwitcher<ITestSwitchableService>>();
            var found = switcher.Value;
            Assert.AreEqual(TestSwitchableKeep.Name, found.NameId, "Should get the correct service and no other");
        }

        [TestMethod]
        public void Has3Services()
        {
            var switcher = GetService<ServiceSwitcher<ITestSwitchableService>>();
            Assert.AreEqual(3, switcher.AllServices.Count, "Should have 3 services to choose from");
        }

        [TestMethod]
        public void NotCreateBeforeButCreatedAfter()
        {
            var switcher = GetService<ServiceSwitcher<ITestSwitchableService>>();
            Assert.IsFalse(switcher.IsValueCreated, "shouldn't be created at first");
            var x = switcher.Value;
            Assert.IsTrue(switcher.IsValueCreated, "should be created afterwards");
        }

        [TestMethod]
        public void FindFallback()
        {
            var switcher = GetService<ServiceSwitcher<ITestSwitchableService>>();
            Assert.AreEqual(TestSwitchableFallback.Name, switcher.ByNameId(TestSwitchableFallback.Name).NameId, "should find by name");
        }
    }
}
