using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Persistence.EFC11.Tests
{
    [TestClass]
    public class ZoneTests
    {
        private EavDbContext _db;

        [TestInitialize]
        public void Init()
        {
            Trace.Write("initializing DB & loader");
            _db = Factory.Resolve<EavDbContext>();
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
