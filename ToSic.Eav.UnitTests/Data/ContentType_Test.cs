using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;

namespace ToSic.Eav.UnitTests
{
    [TestClass]
    public class ContentType_Test
    {
        private const int AppIdX = -1;
        [TestMethod]
        public void ContentType_GeneralTest()
        {
            var x = new ContentType(AppIdX, "SomeName");
            Assert.AreEqual("SomeName", x.Name);
            Assert.AreEqual(null, x.Scope); // not set, should be blank

        }
    }
}
