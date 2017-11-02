using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Types;

namespace ToSic.Eav.Core.Tests.Types
{
    [TestClass]
    public class GlobalManagerTests
    {
        public const int ProvidedTypesCount = 1;

        [TestMethod]
        public void ScanForTypes()
        {
            var globTypes = Eav.Types.Global.ContentTypesInReflection();
            Assert.AreEqual(ProvidedTypesCount, globTypes.Count(), "expect a fixed about of types at dev time");
        }

        [TestMethod]
        public void TestGlobalCache()
        {
            var all = Global.AllContentTypes();
            Assert.AreEqual(ProvidedTypesCount, all.Count);
            var dummy = all.First();
            Assert.AreEqual(DemoType.CTypeName, dummy.Key);
        }

        [TestMethod]
        public void TestInstanceInGlobalCache()
        {
            var dummy = Global.FindContentType(DemoType.CTypeName);
            Assert.AreEqual(DemoType.CTypeName, dummy.StaticName);
        }

        [TestMethod]
        public void CheckDefaultScope()
        {
            var testType = Global.FindContentType(DemoType.CTypeName);
            Assert.AreEqual(TypesBase.UndefinedScope, testType.Scope, "scope should be undefined");
        }

        [TestMethod]
        [Ignore]
        public void TestUndefinedTitle()
        {
            // todo - must create an instance entity to test this

        }

    }
}
