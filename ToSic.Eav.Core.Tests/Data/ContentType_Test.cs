using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;

namespace ToSic.Eav.Core.Tests.Data
{
    [TestClass]
    public class ContentType_Test
    {
        private const int AppIdX = -1;
        [TestMethod]
        public void ContentType_GeneralTest()
        {
            var x = new ContentType(AppIdX, 0, "SomeName");
            Assert.AreEqual("SomeName", x.Name);
            Assert.AreEqual(null, x.Scope); // not set, should be blank

        }
    }
}
