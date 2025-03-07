using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.Persistence.Efc.Tests;

// NOTE: This test doesn't do anything. I assume it should have created a zone and verified that it's created.

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