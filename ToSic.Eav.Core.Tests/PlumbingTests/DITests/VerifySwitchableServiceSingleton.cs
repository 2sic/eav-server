using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Plumbing.DI;

namespace ToSic.Eav.Core.Tests.PlumbingTests.DITests
{
    /// <summary>
    /// Important: the methods are called in A-Z order, so they must preserve the names with the number to achieve this
    /// </summary>
    [TestClass]
    public class VerifySwitchableServiceSingleton: VerifySwitchableServiceBase
    {
        /// <summary>
        /// This must run before everything
        /// </summary>
        [TestMethod]
        public void AccessSingletonN001()
        {
            var switcher = Build<ServiceSwitcherSingleton<ITestSwitchableService>>();
            Assert.IsFalse(switcher.IsValueCreated, "shouldn't be created at first");
        }

        [TestMethod]
        public void AccessSingletonN002()
        {
            var switcher = Build<ServiceSwitcherSingleton<ITestSwitchableService>>();
            Assert.IsFalse(switcher.IsValueCreated, "shouldn't be created at first");
            var x = switcher.Value;
            Assert.IsTrue(switcher.IsValueCreated, "should be created afterwards");
        }

        [TestMethod]
        public void AccessSingletonN003()
        {
            var switcher = Build<ServiceSwitcherSingleton<ITestSwitchableService>>();
            Assert.IsTrue(switcher.IsValueCreated, "should be created by now");
        }

    }
}
