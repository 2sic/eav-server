using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Repositories;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Persistence.Efc.Tests;

[TestClass]
public class Efc11LoadTests : Efc11TestBase
{

    [TestMethod]
    public void GetSomething()
    {
        var results = Db.ToSicEavZones.Single(z => z.ZoneId == 1);
        Assert.IsTrue(results.ZoneId == 1, "zone doesn't fit - it is " + results.ZoneId);
    }

    [Ignore]
    [TestMethod]
    public void TestLoadXAppBlog()
    {
        var results = Loader.AppStateReaderRawTA(2);

        Assert.IsTrue(results.List.Count > 1097 && results.List.Count < 1200, "tried counting entities on the blog-app");
    }

    [Ignore]
    [TestMethod]
    public void PerformanceLoading100XBlog()
    {
        const int loadCount = 25;
        for (var i = 0; i < loadCount; i++)
        {
            Loader = NewLoader();
            Loader.AppStateReaderRawTA(2);
        }
    }

    private const int ExpectedContentTypesOnApp2 = 44;

    [TestMethod]
    public void LoadContentTypesOf2Once()
    {
        var results = TestLoadCts(2);
        Assert.AreEqual(ExpectedContentTypesOnApp2, results.Count, "dummy test: ");
    }

    [TestMethod]
    public void LoadContentTypesOf2TenXCached()
    {
        //Loader.ResetCacheForTesting();
        var results = TestLoadCts(2);
        for (var x = 0; x < 9; x++)
            results = TestLoadCts(2);
        Assert.AreEqual(ExpectedContentTypesOnApp2, results.Count, "dummy test: ");
    }

    [TestMethod]
    public void LoadContentTypesOf2TenXCleared()
    {
        var results = TestLoadCts(2);
        for (var x = 0; x < 9; x++)
        {
            results = TestLoadCts(2);
            //Loader.ResetCacheForTesting();
        }
        // var str = results.ToString();
        Assert.AreEqual(ExpectedContentTypesOnApp2, results.Count, "dummy test: ");
    }

    private IList<IContentType> TestLoadCts(int appId) => (Loader as IRepositoryLoader).ContentTypes(appId, null);
}