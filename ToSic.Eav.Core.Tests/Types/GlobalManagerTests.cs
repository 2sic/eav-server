using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.Core.Tests.Types
{
    [TestClass]
    public class GlobalManagerTests
    {
        public const int ProvidedTypesCount = 1;

        [TestMethod]
        public void ScanForTypes()
        {
            var globTypes = Eav.Types.Global.SystemTypes();
            Assert.AreEqual(ProvidedTypesCount, globTypes.Count(), "expect a fixed about of types at dev time");
        }

        [TestMethod]
        public void CheckDefaultScope()
        {
            var globTypes = Eav.Types.Global.SystemTypes();
            Assert.AreEqual(ProvidedTypesCount, globTypes.Count(), "expect a fixed about of types at dev time");
        }

    }
}
