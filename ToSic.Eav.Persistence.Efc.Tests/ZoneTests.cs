using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.Persistence.Efc.Tests
{
    [TestClass]
    public class ZoneTests
    {

        [TestInitialize]
        public void Init()
        {
            Trace.Write("initializing DB & loader");
        }

        [TestMethod]
        [Ignore]
        public void TestZoneCreate()
        {
            var zid = 0;//Repository.ZoneRepo.Create("create-test" + DateTime.Now);
            Assert.IsTrue(zid != 0, "zone id");


        }
    }
}
