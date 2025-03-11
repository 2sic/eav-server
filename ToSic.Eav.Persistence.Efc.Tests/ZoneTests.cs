using Xunit.Abstractions;

namespace ToSic.Eav.Persistence.Efc.Tests;

// NOTE: This test doesn't do anything. I assume it should have created a zone and verified that it's created.

public class ZoneTests
{
    public ZoneTests(ITestOutputHelper Trace)
    {
        Trace.WriteLine("initializing DB & loader");
    }

    [Fact(Skip = "not active ATM")]
    public void TestZoneCreate()
    {
        var zid = 0;//Repository.ZoneRepo.Create("create-test" + DateTime.Now);
        True(zid != 0, "zone id");
    }
}