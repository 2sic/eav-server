using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav;
using ToSic.Eav.Data;

namespace ToSic.Eav.UnitTests
{
    [TestClass]
    public class ContentType_Test
    {
        [TestMethod]
        public void ContentType_GeneralTest()
        {
            var x = new ContentType("SomeName");
            Assert.AreEqual("SomeName", x.Name);
            Assert.AreEqual(null, x.Scope); // not set, should be blank

        }
    }
}
