using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;

namespace ToSic.Eav.Core.Tests.Data
{
    [TestClass]
    public class DynamicValue_Test
    {
        

        [TestMethod]
        public void TryToLateDeserialize()
        {
            dynamic x = new DynamicValue("{ `idtext`: `7`, `id`: 7}".Replace("`", "\""));

            Assert.AreEqual("7", x.idtext);
            Assert.AreEqual(7, x.id);
            Assert.AreNotEqual(7, x.idtext);
        }
    }
}
