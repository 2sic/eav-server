using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Persistence.EFC11;
using ToSic.Eav.Persistence.EFC11.Models;
using Microsoft.EntityFrameworkCore;

namespace ToSic.Eav.Persistence.EFC11.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void GetSomething()
        {
            using (var db = new EavDbContext()) {
                var results = db.ToSicEavZones.Single(z => z.ZoneId == 1);

                Assert.IsTrue(results.ZoneId == 1);
            }
        }
    }
}
