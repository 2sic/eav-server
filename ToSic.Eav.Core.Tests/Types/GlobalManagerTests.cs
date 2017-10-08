using System;
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
            var globTypes = Eav.Types.Global.SystemTypes();
            Assert.AreEqual(ProvidedTypesCount, globTypes.Count(), "expect a fixed about of types at dev time");
        }

        [TestMethod]
        public void CheckDefaultScope()
        {
            var testType = Global.InstanceOf(DemoType.CTypeName);
            //var globTypes = Eav.Types.Global.SystemTypes();
            //var demoTypeDef = globTypes.First();
            //var testType = (DemoType)Activator.CreateInstance(demoTypeDef);

            Assert.AreEqual(TypesBase.UndefinedScope, testType.Scope, "scope should be undefined");
        }

    }
}
